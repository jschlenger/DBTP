using DBT_API.Entities;
using VDS.RDF;
using DBT_API.Util;
using DBT_API.Enums;
using System.Reflection;
using DBT_API.Settings;

namespace DBT_API.Repositories
{
    public class GraphDBRepository : IGraphRepository
    {
        private readonly SparqlUpdater sparqlConnector;
        private readonly GraphNetwork allGraphs;
        private readonly GraphDbSettings graphDbSettings;
        public GraphDBRepository(SparqlUpdater sparqlConnector, GraphNetwork allGraphs, GraphDbSettings graphDbSettings)
        {
            this.sparqlConnector = sparqlConnector;
            this.allGraphs = allGraphs;
            this.graphDbSettings = graphDbSettings;
        }
        public async Task<IEnumerable<string>> AddNodesAsync(List<Node> nodes, List<Graph> validationGraphs)
        {
            var determineGraphResult = GraphHelper.DetermineGraph(graphDbSettings.BaseRepo, nodes.First().IRI, allGraphs);
            KID kid = determineGraphResult.Item1;
            List<Node> allNodes = determineGraphResult.Item2;

            List<string> addedNodeIRIs = new();
            bool existing = false;
            string problemNode = "";
            foreach (Node node in nodes) 
            {
                Node matchingNode = allNodes.FirstOrDefault(n => n.IRI == node.IRI);
                if (matchingNode != null) 
                {
                    existing = true;
                    problemNode = node.IRI;
                    break;
                }
            }
            if (!existing)
            {
                // perform validation
                Tuple<bool, string> validationResult;

                List<Node> validationNodes = new();
                foreach (Node node in allNodes)
                {
                    Node deepCopyNode = CopyHelper.DeepCopyXML(node);
                    validationNodes.Add(deepCopyNode);
                }
                foreach (Node node in nodes)
                {
                    Node deepCopyNode = CopyHelper.DeepCopyXML(node);
                    validationNodes.Add(deepCopyNode);
                }
                Graph validationDataGraph = new();
                validationDataGraph = GraphHelper.Convert2RDF(validationNodes);
                validationResult = ValidationHelper.CheckForCompliance(validationDataGraph, validationGraphs);

                if (validationResult.Item1) 
                {
                    foreach (Node node in nodes)
                    {
                        addedNodeIRIs.Add(node.IRI);
                    }

                    // perform the update (since the validation was passed successfully)
                    Graph newNodes = new();
                    newNodes = GraphHelper.Convert2RDF(nodes);
                    GraphHelper.UpdateGraphInDb(newNodes, sparqlConnector.ReturnWriteConnector(kid), SparqlUpdate.Add);
                    allNodes.AddRange(nodes);
                    await Task.CompletedTask;
                }
                else
                {
                    addedNodeIRIs.Add(validationResult.Item2);
                }
            }
            return addedNodeIRIs;
        }
        public async Task<Node> GetNodeAsync(string IRI)
        {
            var determineGraphResult = GraphHelper.DetermineGraph(graphDbSettings.BaseRepo, IRI, allGraphs);
            KID kid = determineGraphResult.Item1;
            List<Node> allNodes = determineGraphResult.Item2;

            Node getNode = allNodes.FirstOrDefault(node => node.IRI == IRI);
            await Task.CompletedTask;
            return getNode;
        }
        public async Task<bool> DeleteNodeAsync(string IRI)
        {
            IRI = Uri.UnescapeDataString(IRI);
            var determineGraphResult = GraphHelper.DetermineGraph(graphDbSettings.BaseRepo, IRI, allGraphs);
            KID kid = determineGraphResult.Item1;
            List<Node> allNodes = determineGraphResult.Item2;

            // retain original nodes for later comparison
            List<Node> oldNodes = new();
            foreach (Node node in allNodes)
            {
                Node deepCopyNode = CopyHelper.DeepCopyXML(node);
                oldNodes.Add(deepCopyNode);
            }

            // convert string because of percentage encoding
            IRI = System.Net.WebUtility.UrlDecode(IRI);

            // delete node if IRI is existing
            bool deleted = false;
            var deleteNode = allNodes.FirstOrDefault(node => node.IRI == IRI);
            if (deleteNode != null)
            {
                // remove from in memory list
                allNodes.Remove(deleteNode);

                // remove from GraphDB
                Graph oldGraph = new();
                oldGraph = GraphHelper.Convert2RDF(oldNodes);
                Graph newGraph = new();
                newGraph = GraphHelper.Convert2RDF(allNodes);
                GraphDiffReport diff = oldGraph.Difference(newGraph);
                GraphHelper.UpdateGraphInDb(diff.RemovedTriples, sparqlConnector.ReturnWriteConnector(kid), SparqlUpdate.Delete);

                deleted = true;
            }
            await Task.CompletedTask;
            return deleted;
        }
        public async Task<IEnumerable<string>> UpdateNodesAsync(List<Node> nodes, List<Graph> validationGraphs) 
        {
            var determineGraphResult = GraphHelper.DetermineGraph(graphDbSettings.BaseRepo, nodes.First().IRI, allGraphs);
            KID kid = determineGraphResult.Item1;
            List<Node> allNodes = determineGraphResult.Item2;

            List<string> existingNodeIRIs = new();
            bool existing = true;
            string problemNode = "";

            foreach (Node node in nodes)
            {
                if (allNodes.FirstOrDefault(n => n.IRI == node.IRI) == null)
                {
                    existing = false;
                    problemNode = node.IRI;
                    break;
                }
            }

            if (existing)
            {
                // perform validation
                Tuple<bool, string> validationResult;

                List<Node> validationNodes = new();
                foreach (Node node in allNodes)
                {
                    Node deepCopyNode = CopyHelper.DeepCopyXML(node);
                    validationNodes.Add(deepCopyNode);
                }
                //List<Node> nodesToBeUpdated = new();
                foreach (Node node in nodes)
                {
                    Node matchingNode = validationNodes.FirstOrDefault(n => n.IRI == node.IRI);
                    if (matchingNode != null)
                    {
                        int index = validationNodes.IndexOf(matchingNode);
                        validationNodes[index] = node;
                    }
                }
                Graph validationDataGraph = new();
                validationDataGraph = GraphHelper.Convert2RDF(validationNodes);
                validationResult = ValidationHelper.CheckForCompliance(validationDataGraph, validationGraphs);

                if (validationResult.Item1)
                {
                    foreach (Node node in nodes)
                    {
                        existingNodeIRIs.Add(node.IRI);
                    }

                    Graph newGraph = new();
                    newGraph = GraphHelper.Convert2RDF(nodes);

                    List<Node> nodesToBeUpdated = new();
                    foreach (Node node in nodes)
                    {
                        Node matchingNode = allNodes.FirstOrDefault(n => n.IRI == node.IRI);
                        if (matchingNode != null)
                        {
                            nodesToBeUpdated.Add(matchingNode);
                        }
                    }

                    Graph existingNodes = new();
                    existingNodes = GraphHelper.Convert2RDF(nodesToBeUpdated);

                    GraphDiffReport diff = existingNodes.Difference(newGraph);
                    GraphHelper.UpdateGraphInDb(diff.RemovedTriples, sparqlConnector.ReturnWriteConnector(kid), SparqlUpdate.Delete);
                    GraphHelper.UpdateGraphInDb(diff.AddedTriples, sparqlConnector.ReturnWriteConnector(kid), SparqlUpdate.Add);

                    // loop through all attributes of the nodes that are to be updated and check which ones need to be changed
                    foreach (Node node in nodesToBeUpdated)
                    {
                        Node matchingNode = nodes.FirstOrDefault(n => n.IRI == node.IRI);
                        if (matchingNode.Classes.Count != 0)
                            node.Classes = matchingNode.Classes;
                        if (matchingNode.Relations.Count != 0)
                            node.Relations = matchingNode.Relations;

                        foreach (PropertyInfo prop in node.GetType().GetProperties())
                        {
                            var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                            if (prop.Name != "Classes" && prop.Name != "Domain" && prop.Name != "IRI" && prop.Name != "Relations")
                            {
                                // access the property values
                                var attrValue = prop.GetValue(node);
                                var matchingAttrValue = prop.GetValue(matchingNode);

                                if (propType.Name == "String")
                                {
                                    if (matchingAttrValue != "" && matchingAttrValue != null && matchingAttrValue != " ")
                                        prop.SetValue(node, matchingAttrValue);
                                }
                                else if (propType.Name == "Double")
                                {
                                    if ((double)matchingAttrValue != 0)
                                        prop.SetValue(node, matchingAttrValue);
                                }
                                else if (propType.Name == "Int" || propType.Name == "Int32" || propType.Name == "Int64"
                                        || propType.Name == "UInt" || propType.Name == "UInt32" || propType.Name == "UInt64") 
                                {
                                    if ((int)matchingAttrValue != 0)
                                        prop.SetValue(node, matchingAttrValue);
                                }
                                else if (propType.Name == "DateTime")
                                {
                                    if ((DateTime)matchingAttrValue != new DateTime(1, 1, 1))
                                        prop.SetValue(node, matchingAttrValue);
                                }
                                else if (propType.Name == "Boolean")
                                {

                                }
                            }
                        }
                        // replace node in allNodes
                        var index = allNodes.FindIndex(obj => obj.IRI == node.IRI);
                        allNodes[index] = node;
                    }
                }
                else
                {
                    existingNodeIRIs.Add(validationResult.Item2);
                }
                await Task.CompletedTask;
            }
            return existingNodeIRIs;
        }
    }
}
