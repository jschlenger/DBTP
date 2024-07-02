using DBT_API.Dtos;
using DBT_API.Entities;
using DBT_API.Settings;
using DBT_API.Util;
using InfluxDB.Client;
using InfluxDB.Client.Writes;
using VDS.RDF;
using Microsoft.Office.Interop.Excel;
using System;
using DBT_API.Enums;
using VDS.RDF.Storage;
using System.Security.Cryptography;
using System.Configuration;

namespace DBT_API.Repositories
{
    public class InfluxTimeseriesRepository : ITimeseriesRepository
    {
        private readonly GraphDbSettings graphDbSettings;
        private readonly GraphNetwork allGraphs;
        private readonly InfluxDBClient client;
        private readonly InfluxDbSettings influxDbSettings;
        private readonly SparqlUpdater sparqlConnector;

        public InfluxTimeseriesRepository(GraphNetwork allGraphs, GraphDbSettings graphDbSettings, InfluxDBClient influxDbClient, InfluxDbSettings influxDbSettings, SparqlUpdater sparqlConnector)
        {
            this.allGraphs = allGraphs;
            this.graphDbSettings = graphDbSettings;
            this.client = influxDbClient;
            this.influxDbSettings = influxDbSettings;
            this.sparqlConnector = sparqlConnector;
        }

        public async Task<IEnumerable<Timeseries>> GetTimeseriesAsync(GetTimeseriesDto getTimeseries)
        {
            DateTime currentDateTime = DateTime.Now;

            DataService service = (DataService)allGraphs.Knowledge.FirstOrDefault(n => n.IRI == graphDbSettings.BaseRepo + "_know" + "/dataservice_" + getTimeseries.NodeIRI.Substring(graphDbSettings.BaseRepo.Length + 6));
            string[] identifier = service._identifier.Split(",");
            string database = identifier[0].Split("=")[1]; 
            string measurement = identifier[1].Split("=")[1]; 

            TimeSpan startDifference = currentDateTime - getTimeseries.StartTime;
            string start = FormatTimeDifference(startDifference);
            TimeSpan stopDifference = currentDateTime - getTimeseries.EndTime;
            string stop = FormatTimeDifference(stopDifference);
            string query = "from(bucket: \"" + database + "\")\n" +
                    "|> range(start: " + start + ", stop: " + stop + ")" +
                    "|> filter(fn: (r) => r._measurement == \"" + measurement + "\")";
            foreach (Tuple<string, string> tag in getTimeseries.Tags)
            {
                query += "|> filter(fn: (r) => r." + tag.Item1 + "== \"" + tag.Item2 + "\")";
            }
                    
            var queryResult = await client.GetQueryApi().QueryAsync(query, influxDbSettings.Organisation);

            List<Timeseries> timeseriesResults = new();
            foreach(var result in queryResult.First().Records)
            {
                Timeseries timeseries1 = new();
                DateTime dateTime;
                DateTime.TryParseExact(result.Row.ElementAt(4).ToString(), "yyyy-MM-dd'T'HH:mm:ss'Z'", null, System.Globalization.DateTimeStyles.None, out dateTime);
                timeseries1.Time = dateTime;
                timeseries1.Value = (double)result.Row.ElementAt(5);
                timeseries1.NodeIRI = getTimeseries.NodeIRI;
                timeseries1.Tags = getTimeseries.Tags;
                timeseriesResults.Add(timeseries1);
            }
            return timeseriesResults;
        }

        public async Task<bool> AddTimeseriesByBucketAsync(List<CreateTimeseriesByBucketDto> timeseriesDtos)
        {
            var writeAPI = client.GetWriteApi();

            foreach (var timeseries in timeseriesDtos)
            {
                var entry = PointData.Measurement(timeseries.measurement)
                    .Field("value", timeseries.Value)
                    .Timestamp(timeseries.Time, InfluxDB.Client.Api.Domain.WritePrecision.Ns);
                foreach (var tag in timeseries.Tags)
                {
                    entry = entry.Tag(tag.Item1, tag.Item2);
                }
                writeAPI.WritePoint(entry, timeseries.database, influxDbSettings.Organisation);
            }

            return true;
        }

        public async Task<bool> AddTimeseriesByNodeAsync(List<CreateTimeseriesByNodeDto> timeseriesDtos)
        {
            var writeAPI = client.GetWriteApi();

            foreach (var timeseries in timeseriesDtos)
            {
                string measurement, database;

                Node kpiNode = allGraphs.Knowledge.FirstOrDefault(n => n.IRI == timeseries.nodeIRI);
                DataService service = (DataService)allGraphs.Knowledge.FirstOrDefault(n => n.IRI == graphDbSettings.BaseRepo + "_know" + "/dataservice_" + kpiNode.IRI.Substring(graphDbSettings.BaseRepo.Length + 6));

                var entry = PointData.Measurement(service._identifier.Split(",")[1].Split("=")[1])
                    .Field("value", timeseries.Value)
                    .Timestamp(timeseries.Time, InfluxDB.Client.Api.Domain.WritePrecision.Ns);
                foreach (var tag in timeseries.Tags)
                {
                    entry = entry.Tag(tag.Item1, tag.Item2);
                }
                writeAPI.WritePoint(entry, service._identifier.Split(",")[0].Split("=")[1], influxDbSettings.Organisation);
            }

            return true;
        }

        public async Task<bool> AddTimeseriesConnectionAsync(CreateTimeseriesConnectionDto connectionDto)
        {
            Node node = allGraphs.Knowledge.FirstOrDefault(n => n.IRI == connectionDto.NodeIRI);

            if (node != null)
            {
                List<Node> newNodes = new();

                // create new nodes required to establish connection in the graph
                Dataset dataset = new Dataset
                {
                    Domain = graphDbSettings.BaseRepo + "_know",
                    IRI = graphDbSettings.BaseRepo + "_know" + "/dataset_" + connectionDto.NodeIRI.Substring(graphDbSettings.BaseRepo.Length + 6),
                    Classes = new List<string> { "http://www.w3.org/ns/dcat#Dataset", "https://saref.etsi.org/saref4city/KeyPerformanceIndicatorAssessment" },
                    Relations = new List<Edge> { }
                };

                Edge servesDataset = new Edge
                {
                    Name = "http://www.w3.org/ns/dcat#servesDataset",
                    ObjectIRI = dataset.IRI
                };

                DataService dataservice = new DataService
                {
                    Domain = graphDbSettings.BaseRepo + "_know",
                    IRI = graphDbSettings.BaseRepo + "_know" + "/dataservice_" + connectionDto.NodeIRI.Substring(graphDbSettings.BaseRepo.Length + 6),
                    Classes = new List<string> { "http://www.w3.org/ns/dcat#DataService" },
                    Relations = new List<Edge> { servesDataset },
                    _endpointUrl = connectionDto.EndPoint,
                    _identifier = connectionDto.Identifier 
                };

                newNodes.Add(dataset);
                newNodes.Add(dataservice);

                // convert nodes to RDF
                Graph addGraph = new();
                addGraph = GraphHelper.Convert2RDF(newNodes);

                // add nodes to allNodes
                allGraphs.Knowledge.AddRange(newNodes);
                
                // update graph
                GraphHelper.UpdateGraphInDb(addGraph, sparqlConnector.ReturnWriteConnector(KID.Knowledge), SparqlUpdate.Add);

                // update kpi node to add connection
                Graph existingNodes = new();
                existingNodes = GraphHelper.Convert2RDF(new List<Node> { node });

                Edge quantifiesKPI = new Edge
                {
                    Name = "https://saref.etsi.org/saref4city/quantifiesKPI",
                    ObjectIRI = dataset.IRI
                };
                node.Relations.Add(quantifiesKPI);

                Graph newNodesGraph = new();
                newNodesGraph = GraphHelper.Convert2RDF(new List<Node> { node });

                GraphDiffReport diff = existingNodes.Difference(newNodesGraph);
                GraphHelper.UpdateGraphInDb(diff.RemovedTriples, sparqlConnector.ReturnWriteConnector(KID.Knowledge), SparqlUpdate.Delete);
                GraphHelper.UpdateGraphInDb(diff.AddedTriples, sparqlConnector.ReturnWriteConnector(KID.Knowledge), SparqlUpdate.Add);

                return true;
            }

            return false;
        }

        public string FormatTimeDifference(TimeSpan difference)
        {
            if (difference.TotalDays >= 1)
            {
                int days = (int)difference.TotalDays;
                return $"-{days}d";
            }
            else if (difference.TotalHours >= 1)
            {
                int hours = (int)difference.TotalHours;
                return $"-{hours}h";
            }
            else
            {
                int minutes = (int)difference.TotalMinutes;
                return $"-{minutes}m";
            }
        }
    }
}
