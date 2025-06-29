using DBT_API.Entities;
using DBT_API.Enums;
using DBT_API.Util;
using Minio;
using Minio.DataModel.Args;
using TinyPlyNet;
using VDS.RDF;
using DBT_API.Settings;
using System.IO;
using System.Globalization;

namespace DBT_API.Repositories
{
    public class MinioSetupRepository : ISetupRepository
    {
        private Setup setup = Setup.Empty;
        private readonly IMinioClient minioClient;
        private readonly GraphNetwork allGraphs;
        private readonly SparqlUpdater sparqlConnector;
        private readonly GraphDbSettings graphDbSettings;
        private readonly MinioDbSettings minioDbSettings;

        private readonly string plyDir = "C:\\Users\\example_project\\";

        public MinioSetupRepository(IMinioClient minioClient, GraphNetwork allGraphs, SparqlUpdater sparqlConnector, GraphDbSettings graphDbSettings, MinioDbSettings minioDbSettings)
        {
            this.minioClient = minioClient;
            this.allGraphs = allGraphs;
            this.graphDbSettings = graphDbSettings;
            this.minioDbSettings = minioDbSettings;
            this.sparqlConnector = sparqlConnector;
        }

        public async Task SetBuildingOneAsync(string outputFilePath)
        {
            // find matching ifc and rdf IRIs
            List<string> ifcIds = new();
            List<string> rdfIRIs = new();

            foreach (Node node in allGraphs.Information)
            {
                if (node.GetType() == typeof(Element) || node.GetType().IsSubclassOf(typeof(Element)))
                {
                    ifcIds.Add(((Element)node)._globalIdIfcRoot_attribute_simple);
                    rdfIRIs.Add(node.IRI);
                }
            }

            List<List<string>> matchIds = new();
            matchIds.Add(ifcIds);
            matchIds.Add(rdfIRIs);

            // write matchIDs to file
            using (StreamWriter writer = new StreamWriter(outputFilePath))
            {
                int numEntries = matchIds[0].Count; // Number of entries in each inner list

                for (int i = 0; i < numEntries; i++)
                {
                    List<string> row = new List<string>();

                    foreach (var list in matchIds)
                    {
                        row.Add(list[i]);
                    }

                    string line = string.Join(", ", row);
                    writer.WriteLine(line);
                }
            }

            await Task.CompletedTask;
        }

        public async Task SetBuildingTwoAsync(string IDFilePath, string BBoxFilePath)
        {
            // retain original nodes for later comparison
            List<Node> oldNodes = new();
            foreach (Node node in allGraphs.Information)
            {
                Node deepCopyNode = CopyHelper.DeepCopyXML(node);
                oldNodes.Add(deepCopyNode);
            }

            // read bounding boxes from file
            List<Tuple<string, List<double>>> boundingBoxes = new();
            using (StreamReader reader = new StreamReader(BBoxFilePath))
            {
                string line = reader.ReadLine(); // Read the first line
                if (line != null)
                {
                    // Skip first line if it's just a header
                    if (!line.Contains(','))
                        line = reader.ReadLine();

                    while (line != null)
                    {
                        var parts = line.Split(',');
                        string name = parts[0];
                        var values = new List<double>();

                        for (int i = 1; i < parts.Length; i++)
                        {
                            string valueString = parts[i].Trim();
                            if (double.TryParse(valueString, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out double value))
                            {
                                values.Add(value);
                            }
                            else
                            {
                                Console.WriteLine($"Skipping invalid value: {valueString} in line: {line}");
                            }
                        }

                        boundingBoxes.Add(new Tuple<string, List<double>>(name, values));
                        line = reader.ReadLine();
                    }
                }
            }

            // update nodes with bounding boxes
            List<Node> updatedNodes = new();
            updatedNodes = BBoxHelper.CreateBBoxes(boundingBoxes, allGraphs.Information, graphDbSettings.BaseRepo + "_info/");

            try
            {
                // upload files (maybe switch to octet stream to make things a bit faster)
                DirectoryInfo plyDirInfo = new(plyDir + "ifc2ply\\"); 
                List<FileInfo> files = plyDirInfo.GetFiles().ToList();
                var contentType = "multipart/form-data";

                // Make a bucket on the server, if not already present.
                var beArgs = new BucketExistsArgs()
                    .WithBucket("geometry");
                bool found = await minioClient.BucketExistsAsync(beArgs).ConfigureAwait(false);
                if (!found)
                {
                    var mbArgs = new MakeBucketArgs()
                        .WithBucket("geometry");
                    await minioClient.MakeBucketAsync(mbArgs).ConfigureAwait(false);
                }

                foreach (var file in files)
                {
                    // Upload a file to bucket.
                    var putObjectArgs = new PutObjectArgs()
                        .WithBucket("geometry")
                        .WithObject(file.Name)
                        .WithFileName(file.FullName)
                        .WithContentType(contentType);
                    await minioClient.PutObjectAsync(putObjectArgs).ConfigureAwait(false);
                    Console.WriteLine("Successfully uploaded " + file.Name);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            // add information about where the PLY files are stored
            Dictionary<string, string> geometry2PLY = new();

            // find pairs of geometry iri and ply file name
            DirectoryInfo plyDirInfo2 = new(plyDir + "ifc2ply\\");
            List<FileInfo> files2 = plyDirInfo2.GetFiles().ToList();
            foreach (var file in files2)
            {
                string fileName = file.Name;
                string ifcIri;

                using (var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
                {
                    var f = new PlyFile(stream);
                    ifcIri = f.Comments.Find(x => x.Contains("rdf_iri")).Substring(9);
                }

                var geometryIri = allGraphs.Information
                    .FirstOrDefault(item => item.IRI == ifcIri)?.Relations
                    .FirstOrDefault(edgi => edgi.Name == "https://dtc-ontology.cms.ed.tum.de/ontology/v2#hasGeometry")
                    ?.ObjectIRI;

                if (geometryIri != null)
                {
                    geometry2PLY[geometryIri] = fileName;

                    foreach (var entry in geometry2PLY)
                    {
                        var matchingItem = allGraphs.Information.FirstOrDefault(item => item.IRI == entry.Key);
                        if (matchingItem != null)
                        {
                            ((Geometry)matchingItem)._asPly = minioDbSettings.Host + ":" + minioDbSettings.Port + "/geometry/" + entry.Value.ToString();
                        }
                    }
                }
            }

            Graph oldGraph = new();
            oldGraph = GraphHelper.Convert2RDF(oldNodes);

            Graph newGraph = new();
            newGraph = GraphHelper.Convert2RDF(allGraphs.Information);

            GraphDiffReport diff = oldGraph.Difference(newGraph);
            GraphHelper.UpdateGraphInDb(diff.AddedTriples, sparqlConnector.ReturnWriteConnector(KID.Information), SparqlUpdate.Add);

            setup = Setup.Building;
            await Task.CompletedTask;
        }

        public async Task SetScheduleAsync(string excelFilePath, string ifcToScheduleLinksPath)
        {
            // retain original nodes for later comparison
            List<Node> oldNodes = new();
            foreach (Node node in allGraphs.Information)
            {
                Node deepCopyNode = CopyHelper.DeepCopyXML(node);
                oldNodes.Add(deepCopyNode);
            }

            // find matching ifc and rdf IRIs
            List<string> ifcIds = new();
            List<string> rdfIRIs = new();

            foreach (Node node in allGraphs.Information)
            {
                if (node.GetType() == typeof(Element) || node.GetType().IsSubclassOf(typeof(Element)))
                {
                    ifcIds.Add(((Element)node)._globalIdIfcRoot_attribute_simple);
                    rdfIRIs.Add(node.IRI);
                }
            }

            List<List<string>> matchIds = new()
            {
                ifcIds,
                rdfIRIs
            };

            // initiate node list
            List<Node> asPlannedNodes = new();
            asPlannedNodes = ScheduleHelper.ReadSchedule(excelFilePath, graphDbSettings.BaseRepo + "_info/", ifcToScheduleLinksPath, matchIds);
            allGraphs.Information.AddRange(asPlannedNodes);

            // update graph database
            Graph oldGraph = new();
            oldGraph = GraphHelper.Convert2RDF(oldNodes);

            Graph newGraph = new();
            newGraph = GraphHelper.Convert2RDF(allGraphs.Information);

            GraphDiffReport diff = oldGraph.Difference(newGraph);
            GraphHelper.UpdateGraphInDb(diff.AddedTriples, sparqlConnector.ReturnWriteConnector(KID.Information), SparqlUpdate.Add);

            setup = Setup.Schedule;
            await Task.CompletedTask;
        }
    }
}
