using DBT_API.Entities;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using DBT_API.Settings;
using DBT_API.Enums;
using DBT_API.Util;
using VDS.RDF;
using System;

namespace DBT_API.Repositories
{
    public class MinioBlobRepository : IBlobRepository
    {
        private readonly IMinioClient minioClient;
        private readonly GraphNetwork allGraphs;
        private readonly GraphDbSettings graphDbSettings;
        private readonly SparqlUpdater sparqlConnector;
        private readonly MinioDbSettings minioDbSettings;
        public MinioBlobRepository(IMinioClient minioClient, GraphNetwork allGraphs, GraphDbSettings graphDbSettings, SparqlUpdater sparqlConnector, MinioDbSettings minioDbSettings)
        {
            this.minioClient = minioClient;
            this.allGraphs = allGraphs;
            this.graphDbSettings = graphDbSettings;
            this.sparqlConnector = sparqlConnector;
            this.minioDbSettings = minioDbSettings;
        }
        public async Task<string> AddBlobAsync(Blob blob, IFormFile file)
        {
            // create connections in graph 
            if (blob.NodeIris != null)
            {
                foreach (var iri in blob.NodeIris)
                {
                    KID kid = KID.Data;
                    if (iri.Contains(graphDbSettings.BaseRepo + "_data"))
                    {
                        kid = KID.Data;
                        Node node = allGraphs.Data.FirstOrDefault(n => n.IRI == iri);

                        if (node != null)
                        {
                            List<Node> newNodes = new();

                            // create new nodes required to establish connection in the graph
                            Distribution distribution = new Distribution
                            {
                                Domain = graphDbSettings.BaseRepo + "_data",
                                IRI = graphDbSettings.BaseRepo + "_data" + "/distribution_" + iri.Substring(graphDbSettings.BaseRepo.Length + 6),
                                Classes = new List<string> { "http://www.w3.org/ns/dcat#Distribution" },
                                Relations = new List<Edge> { },
                                _downloadUrl = minioDbSettings.Host + ":" + minioDbSettings.Port + "/" + blob.Bucket + "/" + blob.FileName
                            };

                            Edge distributionEdge = new Edge
                            {
                                Name = "http://www.w3.org/ns/dcat#distribution",
                                ObjectIRI = distribution.IRI
                            };

                            Dataset dataset = new Dataset
                            {
                                Domain = graphDbSettings.BaseRepo + "_data",
                                IRI = graphDbSettings.BaseRepo + "_data" + "/dataset_" + iri.Substring(graphDbSettings.BaseRepo.Length + 6),
                                Classes = new List<string> { "http://www.w3.org/ns/dcat#Dataset" },
                                Relations = new List<Edge> { distributionEdge }
                            };

                            newNodes.Add(dataset);
                            newNodes.Add(distribution);

                            // convert nodes to RDF
                            Graph addGraph = new();
                            addGraph = GraphHelper.Convert2RDF(newNodes);

                            // add nodes to allNodes
                            allGraphs.Data.AddRange(newNodes);

                            // update graph
                            GraphHelper.UpdateGraphInDb(addGraph, sparqlConnector.ReturnWriteConnector(KID.Data), SparqlUpdate.Add);

                            // update kpi node to add connection
                            Graph existingNodes = new();
                            existingNodes = GraphHelper.Convert2RDF(new List<Node> { node });

                            Edge hasResult = new Edge
                            {
                                Name = "http://www.w3.org/ns/sosa/hasResult",
                                ObjectIRI = dataset.IRI
                            };
                            node.Relations.Add(hasResult);

                            Graph newNodesGraph = new();
                            newNodesGraph = GraphHelper.Convert2RDF(new List<Node> { node });

                            GraphDiffReport diff = existingNodes.Difference(newNodesGraph);
                            GraphHelper.UpdateGraphInDb(diff.RemovedTriples, sparqlConnector.ReturnWriteConnector(KID.Data), SparqlUpdate.Delete);
                            GraphHelper.UpdateGraphInDb(diff.AddedTriples, sparqlConnector.ReturnWriteConnector(KID.Data), SparqlUpdate.Add);
                        }
                    }
                    else if (iri.Contains(graphDbSettings.BaseRepo + "_info"))
                    {
                        kid = KID.Information;
                        Node node2 = allGraphs.Information.FirstOrDefault(n => n.IRI == iri);

                        if (node2 != null)
                        {
                            string fileEnding = System.IO.Path.GetExtension(file.FileName);
                            string path = minioDbSettings.Host + ":" + minioDbSettings.Port + "/" + blob.Bucket + "/" + blob.FileName;

                            Graph existingNodes = new();
                            existingNodes = GraphHelper.Convert2RDF(new List<Node> { node2 });

                            if (node2.Classes.Contains("http://www.opengis.net/ont/geosparql#Geometry"))
                            {
                                Geometry geometry = (Geometry)node2;
                                if (fileEnding == ".ply")
                                    geometry._asPly = path;
                                else if (fileEnding == ".ifc")
                                    geometry._asIfc = path;
                                else if (fileEnding == ".obj")
                                    geometry._asObj = path;
                                else if (fileEnding == ".gltf")
                                    geometry._asGltf = path;
                                else if (fileEnding == ".stl")
                                    geometry._asStl = path;
                            }
                            else if (node2.Classes.Contains("https://w3id.org/bot#Element"))
                            {
                                Element element = (Element)node2;
                                if (fileEnding == ".ply")
                                    element._asPly = path;
                                else if (fileEnding == ".ifc")
                                    element._asIfc = path;
                                else if (fileEnding == ".obj")
                                    element._asObj = path;
                                else if (fileEnding == ".gltf")
                                    element._asGltf = path;
                                else if (fileEnding == ".stl")
                                    element._asStl = path;
                            }

                            Graph newNodesGraph = new();
                            newNodesGraph = GraphHelper.Convert2RDF(new List<Node> { node2 });

                            GraphDiffReport diff = existingNodes.Difference(newNodesGraph);
                            GraphHelper.UpdateGraphInDb(diff.RemovedTriples, sparqlConnector.ReturnWriteConnector(kid), SparqlUpdate.Delete);
                            GraphHelper.UpdateGraphInDb(diff.AddedTriples, sparqlConnector.ReturnWriteConnector(kid), SparqlUpdate.Add);
                        }
                    }
                }
            }
            
            // upload blob to object storage
            string etag = null;
            string fullName = null;
            var contentType = "application/octet-stream";
            try
            {
                // Make a bucket on the server, if not already present.
                var beArgs = new BucketExistsArgs()
                    .WithBucket(blob.Bucket);
                bool found = await minioClient.BucketExistsAsync(beArgs).ConfigureAwait(false);
                if (!found)
                {
                    var mbArgs = new MakeBucketArgs()
                        .WithBucket(blob.Bucket);
                    await minioClient.MakeBucketAsync(mbArgs).ConfigureAwait(false);
                }

                using (var stream = file.OpenReadStream())
                {
                    var putObjectArgs = new PutObjectArgs()
                        .WithBucket(blob.Bucket)
                        .WithObject(blob.FileName)
                        .WithStreamData(stream)
                        .WithObjectSize(-1)
                        .WithContentType(contentType);
                    var upload = await minioClient.PutObjectAsync(putObjectArgs);
                    etag = upload.Etag;
                    fullName = upload.ObjectName;
                    //Console.WriteLine("Successfully uploaded " + blob.FileName);
                    return fullName + "/" + etag.Substring(1, etag.Length - 2);
                }
            }
            catch (MinioException e)
            {
                //Console.WriteLine("File Upload Error: {0}", e.Message);
                return "File Upload Error: " + e.Message;
            }
        }

        public async Task<string> GetBlobByNameAsync(string bucket, string fileName)
        {
            try
            {
                StatObjectArgs statObjectArgs = new StatObjectArgs()
                    .WithBucket(bucket)
                    .WithObject(fileName);
                await minioClient.StatObjectAsync(statObjectArgs);

                GetObjectArgs getObjectArgs = new GetObjectArgs()
                    .WithBucket(bucket)
                    .WithObject(fileName)
                    .WithFile("C:\\Users\\example_project\\" + fileName);
                await minioClient.GetObjectAsync(getObjectArgs);
                return "C:\\Users\\example_project\\" + fileName;
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> GetBlobByNodeAsync(string nodeIRI)
        {
            // search graph for connected blobs
            nodeIRI = Uri.UnescapeDataString(nodeIRI);
            bool blobFound = false;
            KID kid = KID.Data;

            string bucket = "";
            string fileName = "";

            if (nodeIRI.Contains(graphDbSettings.BaseRepo + "_data"))
            {
                kid = KID.Data;
                Node node = allGraphs.Data.FirstOrDefault(n => n.IRI == nodeIRI);

                if (node != null)
                {
                    List<Node> newNodes = new();

                    string distributionIRI = graphDbSettings.BaseRepo + "_data" + "/distribution_" + nodeIRI.Substring(graphDbSettings.BaseRepo.Length + 6);
                    string datasetIRI = graphDbSettings.BaseRepo + "_data" + "/dataset_" + nodeIRI.Substring(graphDbSettings.BaseRepo.Length + 6);
                    Dataset dataset = (Dataset)allGraphs.Data.FirstOrDefault(n => n.IRI == datasetIRI);
                    Distribution distribution = (Distribution)allGraphs.Data.FirstOrDefault(n => n.IRI == distributionIRI);

                    bucket = distribution._downloadUrl.Split("/")[1];
                    fileName = distribution._downloadUrl.Split("/")[2];

                    blobFound = true;
                }
            }
            else if (nodeIRI.Contains(graphDbSettings.BaseRepo + "_info"))
            {
                kid = KID.Information;
                Node node2 = allGraphs.Information.FirstOrDefault(n => n.IRI == nodeIRI);

                if (node2 != null)
                {
                    if (node2.Classes.Contains("http://www.opengis.net/ont/geosparql#Geometry"))
                    {
                        Geometry geometry = (Geometry)node2;
                        if (geometry._asPly != null)
                        {
                            fileName = geometry._asPly.Split("/")[2];
                            bucket = geometry._asPly.Split("/")[1];
                        }
                        else if (geometry._asIfc != null)
                        {
                            fileName = geometry._asIfc.Split("/")[2];
                            bucket = geometry._asIfc.Split("/")[1];
                        }
                        else if (geometry._asObj != null)
                        {
                            fileName = geometry._asObj.Split("/")[2];
                            bucket = geometry._asObj.Split("/")[1];
                        }
                        else if (geometry._asGltf != null)
                        {
                            fileName = geometry._asGltf.Split("/")[2];
                            bucket = geometry._asGltf.Split("/")[1];
                        }
                        else if (geometry._asStl != null)
                        {
                            fileName = geometry._asStl.Split("/")[2];
                            bucket = geometry._asStl.Split("/")[1];
                        }
                    }
                    else if (node2.Classes.Contains("https://w3id.org/bot#Element"))
                    {
                        Element element = (Element)node2;
                        if (element._asPly != null)
                        {
                            fileName = element._asPly.Split("/")[2];
                            bucket = element._asPly.Split("/")[1];
                        }
                        else if (element._asIfc != null)
                        {
                            fileName = element._asIfc.Split("/")[2];
                            bucket = element._asIfc.Split("/")[1];
                        }
                        else if (element._asObj != null)
                        {
                            fileName = element._asObj.Split("/")[2];
                            bucket = element._asObj.Split("/")[1];
                        }
                        else if (element._asGltf != null)
                        {
                            fileName = element._asGltf.Split("/")[2];
                            bucket = element._asGltf.Split("/")[1];
                        }
                        else if (element._asStl != null)
                        {
                            fileName = element._asStl.Split("/")[2];
                            bucket = element._asStl.Split("/")[1];
                        }
                    }
                    blobFound = true;
                }
            }

            if (blobFound)
            {
                try
                {
                    StatObjectArgs statObjectArgs = new StatObjectArgs()
                        .WithBucket(bucket)
                        .WithObject(fileName);
                    await minioClient.StatObjectAsync(statObjectArgs);

                    GetObjectArgs getObjectArgs = new GetObjectArgs()
                        .WithBucket(bucket)
                        .WithObject(fileName)
                        .WithFile("C:\\Users\\example_project\\" + fileName);
                    await minioClient.GetObjectAsync(getObjectArgs);
                    return "C:\\Users\\example_project\\" + fileName;
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> DeleteBlobByNameAsync(string bucket, string name)
        {
            RemoveObjectArgs rmArgs = new RemoveObjectArgs()
                .WithBucket(bucket)
                .WithObject(name);
            try
            {
                await minioClient.RemoveObjectAsync(rmArgs);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> DeleteBlobByNodeAsync(string nodeIRI)
        {
            // search graph for connected blobs
            nodeIRI = Uri.UnescapeDataString(nodeIRI);
            bool blobFound = false;
            KID kid = KID.Data;

            string bucket = "";
            string fileName = "";

            if (nodeIRI.Contains(graphDbSettings.BaseRepo + "_data"))
            {
                kid = KID.Data;
                Node node = allGraphs.Data.FirstOrDefault(n => n.IRI == nodeIRI);

                if (node != null)
                {
                    List<Node> newNodes = new();

                    string distributionIRI = graphDbSettings.BaseRepo + "_data" + "/distribution_" + nodeIRI.Substring(graphDbSettings.BaseRepo.Length + 6);
                    string datasetIRI = graphDbSettings.BaseRepo + "_data" + "/dataset_" + nodeIRI.Substring(graphDbSettings.BaseRepo.Length + 6);
                    Dataset dataset = (Dataset)allGraphs.Data.FirstOrDefault(n => n.IRI == datasetIRI);
                    Distribution distribution = (Distribution)allGraphs.Data.FirstOrDefault(n => n.IRI == distributionIRI);

                    bucket = distribution._downloadUrl.Split("/")[1];
                    fileName = distribution._downloadUrl.Split("/")[2];

                    Graph existingNodes = new();
                    existingNodes = GraphHelper.Convert2RDF(allGraphs.Data);

                    allGraphs.Data.Remove(dataset);
                    allGraphs.Data.Remove(distribution);
                    node.Relations.RemoveAll(edge => edge.Name == "http://www.w3.org/ns/sosa/hasResult" && edge.ObjectIRI == datasetIRI) ;

                    // convert nodes to RDF
                    Graph newGraph = new();
                    newGraph = GraphHelper.Convert2RDF(allGraphs.Data);

                    GraphDiffReport diff = existingNodes.Difference(newGraph);
                    GraphHelper.UpdateGraphInDb(diff.RemovedTriples, sparqlConnector.ReturnWriteConnector(KID.Data), SparqlUpdate.Delete);
                    GraphHelper.UpdateGraphInDb(diff.AddedTriples, sparqlConnector.ReturnWriteConnector(KID.Data), SparqlUpdate.Add);

                    blobFound = true;
                }
            }
            else if (nodeIRI.Contains(graphDbSettings.BaseRepo + "_info"))
            {
                kid = KID.Information;
                Node node2 = allGraphs.Information.FirstOrDefault(n => n.IRI == nodeIRI);

                if (node2 != null)
                {
                    Graph existingNodes = new();
                    existingNodes = GraphHelper.Convert2RDF(new List<Node> { node2 });

                    if (node2.Classes.Contains("http://www.opengis.net/ont/geosparql#Geometry"))
                    {
                        Geometry geometry = (Geometry)node2;
                        if (geometry._asPly != null)
                        {
                            fileName = geometry._asPly.Split("/")[2];
                            bucket = geometry._asPly.Split("/")[1];
                            geometry._asPly = null;
                        }
                        else if (geometry._asIfc != null)
                        {
                            fileName = geometry._asIfc.Split("/")[2];
                            bucket = geometry._asIfc.Split("/")[1];
                            geometry._asIfc = null;
                        }
                        else if (geometry._asObj != null)
                        {
                            fileName = geometry._asObj.Split("/")[2];
                            bucket = geometry._asObj.Split("/")[1];
                            geometry._asObj = null;
                        }
                        else if (geometry._asGltf != null)
                        {
                            fileName = geometry._asGltf.Split("/")[2];
                            bucket = geometry._asGltf.Split("/")[1];
                            geometry._asGltf = null;
                        }
                        else if (geometry._asStl != null)
                        {
                            fileName = geometry._asStl.Split("/")[2];
                            bucket = geometry._asStl.Split("/")[1];
                            geometry._asStl = null;
                        }
                    }
                    else if (node2.Classes.Contains("https://w3id.org/bot#Element"))
                    {
                        Element element = (Element)node2;
                        if (element._asPly != null)
                        {
                            fileName = element._asPly.Split("/")[2];
                            bucket = element._asPly.Split("/")[1];
                            element._asPly = null;
                        }
                        else if (element._asIfc != null)
                        {
                            fileName = element._asIfc.Split("/")[2];
                            bucket = element._asIfc.Split("/")[1];
                            element._asIfc = null;
                        }
                        else if (element._asObj != null)
                        {
                            fileName = element._asObj.Split("/")[2];
                            bucket = element._asObj.Split("/")[1];
                            element._asObj = null;
                        }
                        else if (element._asGltf != null)
                        {
                            fileName = element._asGltf.Split("/")[2];
                            bucket = element._asGltf.Split("/")[1];
                            element._asGltf = null;
                        }
                        else if (element._asStl != null)
                        {
                            fileName = element._asStl.Split("/")[2];
                            bucket = element._asStl.Split("/")[1];
                            element._asStl = null;
                        }
                    }

                    Graph newNodesGraph = new();
                    newNodesGraph = GraphHelper.Convert2RDF(new List<Node> { node2 });

                    GraphDiffReport diff = existingNodes.Difference(newNodesGraph);
                    GraphHelper.UpdateGraphInDb(diff.RemovedTriples, sparqlConnector.ReturnWriteConnector(kid), SparqlUpdate.Delete);
                    GraphHelper.UpdateGraphInDb(diff.AddedTriples, sparqlConnector.ReturnWriteConnector(kid), SparqlUpdate.Add);

                    blobFound = true;
                }
            }

            if (blobFound)
            {
                // remove blob
                RemoveObjectArgs rmArgs = new RemoveObjectArgs()
                    .WithBucket(bucket)
                    .WithObject(fileName);
                try
                {
                    await minioClient.RemoveObjectAsync(rmArgs);
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> ConnectBlobAsync(string bucket, string fileName, string nodeIRI)
        {
            nodeIRI = Uri.UnescapeDataString(nodeIRI);
            bool successfulConnected = false;
            KID kid = KID.Data;
            if (nodeIRI.Contains(graphDbSettings.BaseRepo + "_data"))
            {
                kid = KID.Data;
                Node node = allGraphs.Data.FirstOrDefault(n => n.IRI == nodeIRI);

                if (node != null)
                {
                    List<Node> newNodes = new();

                    // create new nodes required to establish connection in the graph
                    Distribution distribution = new Distribution
                    {
                        Domain = graphDbSettings.BaseRepo + "_data",
                        IRI = graphDbSettings.BaseRepo + "_data" + "/distribution_" + nodeIRI.Substring(graphDbSettings.BaseRepo.Length + 6),
                        Classes = new List<string> { "http://www.w3.org/ns/dcat#Distribution" },
                        Relations = new List<Edge> { },
                        _downloadUrl = minioDbSettings.Host + ":" + minioDbSettings.Port + "/" + bucket + "/" + fileName
                    };

                    Edge distributionEdge = new Edge
                    {
                        Name = "http://www.w3.org/ns/dcat#distribution",
                        ObjectIRI = distribution.IRI
                    };

                    Dataset dataset = new Dataset
                    {
                        Domain = graphDbSettings.BaseRepo + "_data",
                        IRI = graphDbSettings.BaseRepo + "_data" + "/dataset_" + nodeIRI.Substring(graphDbSettings.BaseRepo.Length + 6),
                        Classes = new List<string> { "http://www.w3.org/ns/dcat#Dataset" },
                        Relations = new List<Edge> { distributionEdge }
                    };

                    newNodes.Add(dataset);
                    newNodes.Add(distribution);

                    // convert nodes to RDF
                    Graph addGraph = new();
                    addGraph = GraphHelper.Convert2RDF(newNodes);

                    // add nodes to allNodes
                    allGraphs.Data.AddRange(newNodes);

                    // update graph
                    GraphHelper.UpdateGraphInDb(addGraph, sparqlConnector.ReturnWriteConnector(KID.Data), SparqlUpdate.Add);

                    // update kpi node to add connection
                    Graph existingNodes = new();
                    existingNodes = GraphHelper.Convert2RDF(new List<Node> { node });

                    Edge hasResult = new Edge
                    {
                        Name = "http://www.w3.org/ns/sosa/hasResult",
                        ObjectIRI = dataset.IRI
                    };
                    node.Relations.Add(hasResult);

                    Graph newNodesGraph = new();
                    newNodesGraph = GraphHelper.Convert2RDF(new List<Node> { node });

                    GraphDiffReport diff = existingNodes.Difference(newNodesGraph);
                    GraphHelper.UpdateGraphInDb(diff.RemovedTriples, sparqlConnector.ReturnWriteConnector(KID.Data), SparqlUpdate.Delete);
                    GraphHelper.UpdateGraphInDb(diff.AddedTriples, sparqlConnector.ReturnWriteConnector(KID.Data), SparqlUpdate.Add);

                    successfulConnected = true;
                }
            }
            else if (nodeIRI.Contains(graphDbSettings.BaseRepo + "_info"))
            {
                kid = KID.Information;
                Node node2 = allGraphs.Information.FirstOrDefault(n => n.IRI == nodeIRI);

                if (node2 != null)
                {
                    string fileEnding = "." + fileName.Split(".")[1];
                    string path = minioDbSettings.Host + ":" + minioDbSettings.Port + "/" + bucket + "/" + fileName;

                    Graph existingNodes = new();
                    existingNodes = GraphHelper.Convert2RDF(new List<Node> { node2 });

                    if (node2.Classes.Contains("http://www.opengis.net/ont/geosparql#Geometry"))
                    {
                        Geometry geometry = (Geometry)node2;
                        if (fileEnding == ".ply")
                            geometry._asPly = path;
                        else if (fileEnding == ".ifc")
                            geometry._asIfc = path;
                        else if (fileEnding == ".obj")
                            geometry._asObj = path;
                        else if (fileEnding == ".gltf")
                            geometry._asGltf = path;
                        else if (fileEnding == ".stl")
                            geometry._asStl = path;
                    }
                    else if (node2.Classes.Contains("https://w3id.org/bot#Element"))
                    {
                        Element element = (Element)node2;
                        if (fileEnding == ".ply")
                            element._asPly = path;
                        else if (fileEnding == ".ifc")
                            element._asIfc = path;
                        else if (fileEnding == ".obj")
                            element._asObj = path;
                        else if (fileEnding == ".gltf")
                            element._asGltf = path;
                        else if (fileEnding == ".stl")
                            element._asStl = path;
                    }

                    Graph newNodesGraph = new();
                    newNodesGraph = GraphHelper.Convert2RDF(new List<Node> { node2 });

                    GraphDiffReport diff = existingNodes.Difference(newNodesGraph);
                    GraphHelper.UpdateGraphInDb(diff.RemovedTriples, sparqlConnector.ReturnWriteConnector(kid), SparqlUpdate.Delete);
                    GraphHelper.UpdateGraphInDb(diff.AddedTriples, sparqlConnector.ReturnWriteConnector(kid), SparqlUpdate.Add);

                    successfulConnected = true;
                }
            }

            return successfulConnected;
        }
        public async Task GetBlobsAsync(Guid id)
        {
            await Task.CompletedTask;
        }
    }
}
