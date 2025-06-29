using System.Globalization;
using DBT_API.Entities;

namespace DBT_API.Util
{
    public static class BBoxHelper
    {
        public static List<Node> CreateBBoxes(List<Tuple<string, List<double>>> boundingBoxes, List<Node> nodes, string domain)
        {
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";

            List<Tuple<string, List<double>>> bBoxesNoDup = new List<Tuple<string, List<double>>>();
            bool alreadyExisting = false;
            int ID = 1;
            for (int i = 0; i < boundingBoxes.Count(); i++)
            {
                for (int j = 0; j < bBoxesNoDup.Count(); j++)
                {
                    if (bBoxesNoDup.ElementAt(j).Item1 == boundingBoxes.ElementAt(i).Item1)
                    {
                        alreadyExisting = true;
                        if (bBoxesNoDup.ElementAt(j).Item2.ElementAt(0) > boundingBoxes.ElementAt(i).Item2.ElementAt(0))
                            bBoxesNoDup.ElementAt(j).Item2[0] = boundingBoxes.ElementAt(i).Item2.ElementAt(0);
                        if (bBoxesNoDup.ElementAt(j).Item2.ElementAt(1) > boundingBoxes.ElementAt(i).Item2.ElementAt(1))
                            bBoxesNoDup.ElementAt(j).Item2[1] = boundingBoxes.ElementAt(i).Item2.ElementAt(1);
                        if (bBoxesNoDup.ElementAt(j).Item2.ElementAt(2) > boundingBoxes.ElementAt(i).Item2.ElementAt(2))
                            bBoxesNoDup.ElementAt(j).Item2[2] = boundingBoxes.ElementAt(i).Item2.ElementAt(2);
                        if (bBoxesNoDup.ElementAt(j).Item2.ElementAt(3) < boundingBoxes.ElementAt(i).Item2.ElementAt(3))
                            bBoxesNoDup.ElementAt(j).Item2[3] = boundingBoxes.ElementAt(i).Item2.ElementAt(3);
                        if (bBoxesNoDup.ElementAt(j).Item2.ElementAt(4) < boundingBoxes.ElementAt(i).Item2.ElementAt(4))
                            bBoxesNoDup.ElementAt(j).Item2[4] = boundingBoxes.ElementAt(i).Item2.ElementAt(4);
                        if (bBoxesNoDup.ElementAt(j).Item2.ElementAt(5) < boundingBoxes.ElementAt(i).Item2.ElementAt(5))
                            bBoxesNoDup.ElementAt(j).Item2[5] = boundingBoxes.ElementAt(i).Item2.ElementAt(5);
                    }
                }

                if (!alreadyExisting)
                {
                    bBoxesNoDup.Add(boundingBoxes.ElementAt(i));
                }

                alreadyExisting = false;
            }

            //List<Geometry> geometries = new List<Geometry>();
            foreach (Tuple<string, List<double>> box in bBoxesNoDup)
            {
                string asWKT = "POLYHEDRALSURFACE Z (";

                asWKT += "((" + box.Item2[0].ToString(nfi) + " " + box.Item2[1].ToString(nfi) + " " + box.Item2[2].ToString(nfi) + ", ";
                asWKT += box.Item2[0].ToString(nfi) + " " + box.Item2[4].ToString(nfi) + " " + box.Item2[2].ToString(nfi) + ", ";
                asWKT += box.Item2[0].ToString(nfi) + " " + box.Item2[4].ToString(nfi) + " " + box.Item2[5].ToString(nfi) + ", ";
                asWKT += box.Item2[0].ToString(nfi) + " " + box.Item2[1].ToString(nfi) + " " + box.Item2[5].ToString(nfi) + ", ";
                asWKT += box.Item2[0].ToString(nfi) + " " + box.Item2[1].ToString(nfi) + " " + box.Item2[2].ToString(nfi) + ")),";

                asWKT += "((" + box.Item2[0].ToString(nfi) + " " + box.Item2[1].ToString(nfi) + " " + box.Item2[2].ToString(nfi) + ", ";
                asWKT += box.Item2[3].ToString(nfi) + " " + box.Item2[1].ToString(nfi) + " " + box.Item2[2].ToString(nfi) + ", ";
                asWKT += box.Item2[3].ToString(nfi) + " " + box.Item2[4].ToString(nfi) + " " + box.Item2[2].ToString(nfi) + ", ";
                asWKT += box.Item2[0].ToString(nfi) + " " + box.Item2[4].ToString(nfi) + " " + box.Item2[2].ToString(nfi) + ", ";
                asWKT += box.Item2[0].ToString(nfi) + " " + box.Item2[1].ToString(nfi) + " " + box.Item2[2].ToString(nfi) + ")),";

                asWKT += "((" + box.Item2[0].ToString(nfi) + " " + box.Item2[1].ToString(nfi) + " " + box.Item2[2].ToString(nfi) + ", ";
                asWKT += box.Item2[0].ToString(nfi) + " " + box.Item2[1].ToString(nfi) + " " + box.Item2[5].ToString(nfi) + ", ";
                asWKT += box.Item2[3].ToString(nfi) + " " + box.Item2[1].ToString(nfi) + " " + box.Item2[5].ToString(nfi) + ", ";
                asWKT += box.Item2[3].ToString(nfi) + " " + box.Item2[1].ToString(nfi) + " " + box.Item2[2].ToString(nfi) + ", ";
                asWKT += box.Item2[0].ToString(nfi) + " " + box.Item2[1].ToString(nfi) + " " + box.Item2[2].ToString(nfi) + ")),";

                asWKT += "((" + box.Item2[0].ToString(nfi) + " " + box.Item2[1].ToString(nfi) + " " + box.Item2[5].ToString(nfi) + ", ";
                asWKT += box.Item2[0].ToString(nfi) + " " + box.Item2[4].ToString(nfi) + " " + box.Item2[5].ToString(nfi) + ", ";
                asWKT += box.Item2[3].ToString(nfi) + " " + box.Item2[4].ToString(nfi) + " " + box.Item2[5].ToString(nfi) + ", ";
                asWKT += box.Item2[3].ToString(nfi) + " " + box.Item2[1].ToString(nfi) + " " + box.Item2[5].ToString(nfi) + ", ";
                asWKT += box.Item2[0].ToString(nfi) + " " + box.Item2[1].ToString(nfi) + " " + box.Item2[5].ToString(nfi) + ")),";

                asWKT += "((" + box.Item2[3].ToString(nfi) + " " + box.Item2[1].ToString(nfi) + " " + box.Item2[2].ToString(nfi) + ", ";
                asWKT += box.Item2[3].ToString(nfi) + " " + box.Item2[1].ToString(nfi) + " " + box.Item2[5].ToString(nfi) + ", ";
                asWKT += box.Item2[3].ToString(nfi) + " " + box.Item2[4].ToString(nfi) + " " + box.Item2[5].ToString(nfi) + ", ";
                asWKT += box.Item2[3].ToString(nfi) + " " + box.Item2[4].ToString(nfi) + " " + box.Item2[2].ToString(nfi) + ", ";
                asWKT += box.Item2[3].ToString(nfi) + " " + box.Item2[1].ToString(nfi) + " " + box.Item2[2].ToString(nfi) + ")),";

                asWKT += "((" + box.Item2[0].ToString(nfi) + " " + box.Item2[4].ToString(nfi) + " " + box.Item2[2].ToString(nfi) + ", ";
                asWKT += box.Item2[0].ToString(nfi) + " " + box.Item2[4].ToString(nfi) + " " + box.Item2[5].ToString(nfi) + ", ";
                asWKT += box.Item2[3].ToString(nfi) + " " + box.Item2[4].ToString(nfi) + " " + box.Item2[5].ToString(nfi) + ", ";
                asWKT += box.Item2[3].ToString(nfi) + " " + box.Item2[4].ToString(nfi) + " " + box.Item2[2].ToString(nfi) + ", ";
                asWKT += box.Item2[0].ToString(nfi) + " " + box.Item2[4].ToString(nfi) + " " + box.Item2[2].ToString(nfi) + ")))";

                Geometry geom = new Geometry
                {
                    Domain = domain,
                    IRI = domain + "geometry" + ID.ToString(),
                    Classes = new List<string> { "http://www.opengis.net/ont/geosparql#Geometry" },
                    _id = ID,
                    _asWKT = asWKT,
                };

                //geometries.Add(geom);
                nodes.Add(geom);

                for (int i = 0; i < nodes.Count(); i++)
                {
                    if (nodes.ElementAt(i).GetType().IsSubclassOf(typeof(Element)))
                    {
                        if (((Element)nodes.ElementAt(i))._globalIdIfcRoot_attribute_simple == box.Item1)
                        {
                            Edge edge = CreateEdge("https://dtc-ontology.cms.ed.tum.de/ontology/v2#hasGeometry", domain + "geometry" + ID.ToString());
                            if (nodes.ElementAt(i).Relations != null)
                                nodes.ElementAt(i).Relations.Add(edge);
                            else
                                nodes.ElementAt(i).Relations = new List<Edge> { edge };
                        }
                    }
                }

                ID = ++ID;
            }

            return nodes;
        }

        public static Edge CreateEdge(string name, string iri)
        {
            Edge edge = new Edge
            {
                Name = name,
                ObjectIRI = iri
            };
            return edge;
        }

    }
}
