using AngleSharp.Text;
using DBT_API.Entities;
using Newtonsoft.Json;
using System.Globalization;
using System.Reflection;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Update;
using DBT_API.Enums;

namespace DBT_API.Util
{
    public static class GraphHelper
    {

        public static void RemoveSubjectObjectDoubles(IGraph graph)
        {
            // Create a list to store triples to remove
            List<Triple> triplesToRemove = new();

            // Identify triples to remove
            foreach (Triple triple in graph.Triples)
            {
                if (triple.Object.Equals(triple.Subject))
                {
                    triplesToRemove.Add(triple);
                }
            }

            // Remove the identified triples
            foreach (Triple tripleToRemove in triplesToRemove)
            {
                graph.Retract(tripleToRemove);
            }
        }
        public static void RemoveTriples(IGraph graph, string objects)
        {
            // Example: Remove triples with a specific predicate
            IUriNode objectToRemove = graph.CreateUriNode(new Uri(objects));

            // Create a list to store triples to remove
            List<Triple> triplesToRemove = new();

            // Identify triples to remove
            foreach (Triple triple in graph.Triples)
            {
                if (triple.Object.Equals(objectToRemove))
                {
                    triplesToRemove.Add(triple);
                }
            }

            // Remove the identified triples
            foreach (Triple tripleToRemove in triplesToRemove)
            {
                graph.Retract(tripleToRemove);
            }
        }
        public static void Convert2Csharp(Graph graph, List<Node> allNodes)
        {
            List<INode> uniqueNodes = new();
            foreach (var node in graph.Triples)
            {
                bool alreadyExisting = false;
                foreach (INode uniqueNode in uniqueNodes)
                {
                    if (uniqueNode.ToString() == node.Subject.ToString())
                    {
                        alreadyExisting = true;
                        break;
                    }
                }
                if (!alreadyExisting)
                {
                    uniqueNodes.Add(node.Subject);
                }
            }

            foreach (INode uniqueNode in uniqueNodes)
            {
                foreach (var node in graph.Triples)
                {
                    if (node.Subject.ToString() == uniqueNode.ToString())
                    {
                        if (node.Predicate.ToString() == "http://www.w3.org/1999/02/22-rdf-syntax-ns#type" && node.Object.ToString() != "https://w3id.org/bot#Element") // how to handle multiple types // check which is the most specific subclass
                        {
                            string classi = "";
                            int hashIndex = node.Object.ToString().IndexOf('#');
                            if (hashIndex != -1)
                            {
                                classi = node.Object.ToString().Substring(hashIndex + 1);
                            }
                            else
                            {
                                int slashIndex = node.Object.ToString().LastIndexOf('/');
                                if (slashIndex != -1)
                                {
                                    classi = node.Object.ToString().Substring(slashIndex + 1);
                                }
                            }

                            classi = classi.Split('-')[0];
                            if (classi == "Task")
                                classi += "i";
                            classi = "DBT_API.Entities." + classi;
                            //Console.WriteLine(classi);

                            Node nodecs = CreateClassInstance(classi);
                            nodecs.Classes = new List<string> { };
                            nodecs.Relations = new List<Edge> { };
                            nodecs.IRI = node.Subject.ToString();
                            //nodecs.Domain = node.Subject.ToString().Substring((node.Subject.ToString().LastIndexOf('/')) + 1);
                            foreach (var triple in graph.Triples)
                            {
                                if (triple.Subject.ToString() == uniqueNode.ToString())
                                {
                                    if (triple.Object.NodeType == VDS.RDF.NodeType.Literal)
                                    {
                                        // handle attribute
                                        string attributeName = "";
                                        int hashIndex2 = triple.Predicate.ToString().IndexOf('#');
                                        if (hashIndex2 != -1)
                                        {
                                            attributeName = triple.Predicate.ToString().Substring(hashIndex2 + 1);
                                        }
                                        else
                                        {
                                            int slashIndex = triple.Predicate.ToString().LastIndexOf('/');
                                            if (slashIndex != -1)
                                            {
                                                attributeName = triple.Predicate.ToString().Substring(slashIndex + 1);
                                            }
                                        }
                                        attributeName = "_" + attributeName;
                                        LiteralNode literal = (LiteralNode)triple.Object;
                                        if (literal.DataType == null)
                                        {
                                            string attributeValue = literal.ToString();
                                            SetProperty(nodecs, attributeName, attributeValue);
                                        }
                                        else
                                        {
                                            var attributetype = literal.DataType.ToString().Split('#')[1];
                                            attributetype = attributetype[0].ToString().ToUpper() + attributetype.Substring(1);
                                            Console.WriteLine(attributetype);
                                            if (attributetype == "Double")
                                            {
                                                double attributeValue = literal.Value.ToDouble();
                                                SetProperty(nodecs, attributeName, attributeValue);
                                            }
                                            else if(attributetype == "String")
                                            {
                                                string attributeValue = literal.Value.ToString();
                                                SetProperty(nodecs, attributeName, attributeValue);
                                            }
                                            else if(attributetype == "Integer" || attributetype == "Int")
                                            {
                                                int attributeValue = Int32.Parse(literal.Value);
                                                SetProperty(nodecs, attributeName, attributeValue);
                                            }
                                        }
                                    }
                                    else if (triple.Predicate.ToString() == "http://www.w3.org/1999/02/22-rdf-syntax-ns#type")
                                    {
                                        // handle type 
                                        nodecs.Classes.Add(triple.Object.ToString());
                                    }
                                    else
                                    {
                                        // handle relationships
                                        Edge edge = new();
                                        edge.ObjectIRI = triple.Object.ToString();
                                        edge.Name = triple.Predicate.ToString();
                                        nodecs.Relations.Add(edge);
                                    }
                                }
                            }
                            allNodes.Add(nodecs);
                            break;
                        }
                    }
                }
            }
        }

        public static Graph Convert2RDF(List<Node> allNodes)
        {
            Graph graph = new();
            foreach (Node node in allNodes)
            {
                // set IRI
                IUriNode self = graph.CreateUriNode(UriFactory.Create(node.IRI));

                // define class triple
                IUriNode type = graph.CreateUriNode(UriFactory.Create("http://www.w3.org/1999/02/22-rdf-syntax-ns#type"));

                foreach (string classi in node.Classes)
                {
                    IUriNode classNode = graph.CreateUriNode(UriFactory.Create(classi));
                    graph.Assert(new Triple(self, type, classNode));
                }

                // define attribute triples
                foreach (PropertyInfo prop in node.GetType().GetProperties())
                {
                    var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                    if (prop.Name != "Classes" && prop.Name != "Domain" && prop.Name != "IRI" && prop.Name != "Relations")
                    {
                        // access the JsonProperty -> PropertyName
                        JsonPropertyAttribute propy = prop.GetCustomAttributes(typeof(JsonPropertyAttribute), true).FirstOrDefault() as JsonPropertyAttribute;
                        IUriNode predicate = graph.CreateUriNode(UriFactory.Create(propy.PropertyName));

                        // define object
                        ILiteralNode objecti;
                        var attrValue = prop.GetValue(node);

                        // select correct decimal separator
                        NumberFormatInfo nfi = new();
                        nfi.NumberDecimalSeparator = ".";

                        if (attrValue != null)
                        {
                            if (prop.Name == "_id" && attrValue.ToString() == "0")
                            {

                            }

                            else if (propType == typeof(DateTime))
                            {
                                //objecti = new DateTime().ToLiteral(graph); // missing value
                                objecti = graph.CreateLiteralNode(attrValue.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDateTime));
                                graph.Assert(new Triple(self, predicate, objecti));
                            }
                            else if (propType == typeof(int))
                            {
                                //objecti = ().ToLiteral(graph);
                                objecti = graph.CreateLiteralNode(attrValue.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger));
                                graph.Assert(new Triple(self, predicate, objecti));
                            }
                            else if (propType == typeof(double))
                            {
                                //objecti = new Double().ToLiteral(graph);
                                double doubleValue = (double)attrValue;
                                objecti = graph.CreateLiteralNode(doubleValue.ToString(nfi), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble));
                                graph.Assert(new Triple(self, predicate, objecti));
                            }
                            else if (propType == typeof(string))
                            {
                                objecti = graph.CreateLiteralNode(attrValue.ToString());
                                //objecti = graph.CreateLiteralNode(attrValue.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeString));
                                graph.Assert(new Triple(self, predicate, objecti));
                            }
                            else if (propType == typeof(bool))
                            {
                                objecti = graph.CreateLiteralNode(attrValue.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
                                graph.Assert(new Triple(self, predicate, objecti));
                            }
                        }

                        //Console.WriteLine(propy.PropertyName + " " + propType.ToString());
                    }
                }

                // define relationship triples
                if (node.Relations != null)
                {
                    foreach (Edge outi in node.Relations)
                    {
                        IUriNode targetIri = graph.CreateUriNode(UriFactory.Create(outi.ObjectIRI));
                        IUriNode relationship = graph.CreateUriNode(UriFactory.Create(outi.Name));
                        graph.Assert(new Triple(self, relationship, targetIri));
                    }
                }
            }

            return graph;
        }

        public static string ConstructSparqlUpdateQuery(List<Triple> triples, SparqlUpdate update)
        {
            StringBuilder queryBuilder = new();
            //queryBuilder.AppendLine("PREFIX ex: <http://example.org/>");
            if (update == SparqlUpdate.Add)
                queryBuilder.AppendLine("INSERT {");
            else if (update == SparqlUpdate.Delete)
                queryBuilder.AppendLine("DELETE {");

            foreach (Triple triple in triples)
            {
                string start;
                if (triple.Object.ToString().Length >= 4)
                {
                    start = triple.Object.ToString().Substring(0, 4);
                }
                else
                {
                    start = triple.Object.ToString();
                }
                if ((start == "http" || start == "urn:") && !triple.Object.ToString().Contains("^^"))
                {
                    queryBuilder.AppendLine($"  <{triple.Subject}> <{triple.Predicate}> <{triple.Object}> .");
                }
                else
                {
                    ILiteralNode literalNode = (ILiteralNode)triple.Object;
                    if (literalNode.DataType != null)
                    {
                        string datatype = literalNode.DataType.ToString();
                        queryBuilder.AppendLine($" <{triple.Subject}> <{triple.Predicate}> \"{literalNode.Value}\"^^<{datatype}> .");
                    }
                    else
                    {
                        queryBuilder.AppendLine($" <{triple.Subject}> <{triple.Predicate}> \"{triple.Object}\" .");
                    }
                }
            }

            queryBuilder.AppendLine("} ");
            queryBuilder.AppendLine("WHERE { }");

            return queryBuilder.ToString();
        }

        public static void SendSparqlUpdateQuery(SparqlRemoteUpdateEndpoint updateEndpoint, string query)
        {
            SparqlParameterizedString update = new();
            update.CommandText = query;

            string modifiedString = query.Replace("\\", "\\\\");
            Console.Write(modifiedString);

            // Execute the update
            updateEndpoint.Update(modifiedString);
            Console.WriteLine("SPARQL UPDATE query executed successfully.");
        }

        public static void UpdateGraphInDb(Graph newNodes, SparqlRemoteUpdateEndpoint writeConnector, SparqlUpdate update)
        {
            int stepsize = 100;
            List<Triple> tripleBatch;
            int nrOfTriples = newNodes.Triples.Count;
            for (int i = 0; nrOfTriples > i; i += stepsize)
            {
                int remaining = newNodes.Triples.Count - i;
                if (remaining > 100)
                    remaining = 100;
                tripleBatch = new List<Triple>();
                for (int j = 0; j < remaining; j++)
                {
                    tripleBatch.Add(newNodes.Triples.ElementAt(i + j));
                }
                string querystring = ConstructSparqlUpdateQuery(tripleBatch, update);
                SendSparqlUpdateQuery(writeConnector, querystring);
            }
        }

        public static void UpdateGraphInDb(IEnumerable<Triple> newNodes, SparqlRemoteUpdateEndpoint writeConnector, SparqlUpdate update)
        {
            int stepsize = 100;
            List<Triple> tripleBatch;
            int nrOfTriples = newNodes.Count();
            for (int i = 0; nrOfTriples > i; i += stepsize)
            {
                int remaining = newNodes.Count() - i;
                if (remaining > 100)
                    remaining = 100;
                tripleBatch = new List<Triple>();
                for (int j = 0; j < remaining; j++)
                {
                    tripleBatch.Add(newNodes.ElementAt(i + j));
                }
                string querystring = ConstructSparqlUpdateQuery(tripleBatch, update);
                SendSparqlUpdateQuery(writeConnector, querystring);
            }
        }

        public static Node CreateClassInstance(string classString)
        {
            Type t = Type.GetType(classString);
            return (Node)Activator.CreateInstance(t);
        }

        public static void SetProperty<T>(Node node, string propName, T value)
        {
            Type _classtype = node.GetType();
            PropertyInfo _propertyInfo = _classtype.GetProperty(propName);
            _propertyInfo.SetValue(node, value);
        }

        public static Tuple<KID, List<Node>> DetermineGraph(string domain, string IRI, GraphNetwork allGraphs)
        {
            KID kid = KID.Empty;
            List<Node> allNodes = new();
            if (IRI.Contains(domain + "_data"))
            {
                kid = KID.Data;
                allNodes = allGraphs.Data;
            }
            else if (IRI.Contains(domain + "_info"))
            {
                kid = KID.Information;
                allNodes = allGraphs.Information;
            }
            else if (IRI.Contains(domain + "_know"))
            {
                kid = KID.Knowledge;
                allNodes = allGraphs.Knowledge;
            }
            return new Tuple<KID, List<Node>>(kid, allNodes);
        }
    }
}
