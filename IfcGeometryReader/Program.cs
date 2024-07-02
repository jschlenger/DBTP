using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using Xbim.Common.XbimExtensions;
using Xbim.Ifc;
using Xbim.ModelGeometry.Scene;
using Xbim.Common.Geometry;
using TinyPlyNet;
using System.Globalization;

namespace IfcGeometryReader
{
    internal class Program
    {

        static void Main(string[] args)
        {
            string IDFilePath = "C:\\examples\\IDs.txt";
            string ifcFilePath = "C:\\examples\\example_buildings.ifc";
            string outputPlyDirPath = "C:\\examples\\ifc2ply\\";
            string outputBBoxPath = "C:\\examples\\BBoxes.txt";

            List<List<string>> IDs = new List<List<string>>();
            using (StreamReader reader = new StreamReader(IDFilePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] values = line.Split(new[] { ", " }, StringSplitOptions.None);
                    for (int i = 0; i < values.Length; i++)
                    {
                        if (IDs.Count <= i)
                        {
                            IDs.Add(new List<string>());
                        }
                        IDs[i].Add(values[i]);
                    }
                }
            }

            List<Tuple<string, List<double>>> boundingBoxes = new List<Tuple<string, List<double>>>();

            using (var model = IfcStore.Open(ifcFilePath))
            {
                var schemaVersion = model.SchemaVersion;

                double meterConversion = model.ModelFactors.LengthToMetresConversionFactor;

                Xbim3DModelContext modelContext = new Xbim3DModelContext(model);
                modelContext.CreateContext();
                List<XbimShapeInstance> shapeInstances = modelContext.ShapeInstances().ToList();

                int counter = 0;
                foreach (var instance in shapeInstances)
                {
                    if (instance.RepresentationType == XbimGeometryRepresentationType.OpeningsAndAdditionsIncluded)
                    {
                        var instanceTransform = instance.Transformation;

                        XbimShapeGeometry geometry = modelContext.ShapeGeometry(instance);
                        XbimRect3D boundingBox = geometry.BoundingBox.Transform(instanceTransform);

                        List<double> bounds = new List<double>();
                        bounds.Add(boundingBox.Min.X * meterConversion);
                        bounds.Add(boundingBox.Min.Y * meterConversion);
                        bounds.Add(boundingBox.Min.Z * meterConversion);
                        bounds.Add(boundingBox.Max.X * meterConversion);
                        bounds.Add(boundingBox.Max.Y * meterConversion);
                        bounds.Add(boundingBox.Max.Z * meterConversion);

                        string ID;
                        if (schemaVersion.ToString() == "Ifc4")
                        {
                            var element = model.Instances.Where<Xbim.Ifc4.Kernel.IfcProduct>(w => w.EntityLabel == instance.IfcProductLabel);
                            ID = element.First().GlobalId;
                        }
                        else
                        {
                            var element = model.Instances.Where<Xbim.Ifc2x3.Kernel.IfcProduct>(w => w.EntityLabel == instance.IfcProductLabel);
                            ID = element.First().GlobalId;
                        }

                        Tuple<string, List<double>> tuple = new Tuple<string, List<double>>(ID, bounds);
                        boundingBoxes.Add(tuple);

                        byte[] data = ((IXbimShapeGeometryData)geometry).ShapeData;

                        using (var stream = new MemoryStream(data))
                        {
                            using (var reader = new BinaryReader(stream))
                            {
                                var productMesh = reader.ReadShapeTriangulation();

                                List<XbimFaceTriangulation> faces = productMesh.Faces as List<XbimFaceTriangulation>;
                                List<XbimPoint3D> vertices = productMesh.Vertices as List<XbimPoint3D>;

                                // get globalID and class name of the object the geometry belongs to
                                string className;
                                string name;
                                string globalID;
                                string rdfIRI;
                                if (schemaVersion.ToString() == "Ifc4")
                                {
                                    var element = model.Instances.Where<Xbim.Ifc4.Kernel.IfcProduct>(w => w.EntityLabel == instance.IfcProductLabel);
                                    globalID = element.First().GlobalId;
                                    rdfIRI = "xxxxx";
                                    for (int i = 0; i < IDs.ElementAt(0).Count(); i++)
                                    {
                                        if (IDs.ElementAt(0).ElementAt(i) == globalID)
                                        {
                                            rdfIRI = IDs.ElementAt(1).ElementAt(i);
                                            break;
                                        }
                                    }
                                    name = element.First().Name;
                                    className = element.First().GetType().Name;
                                }
                                else
                                {
                                    var element = model.Instances.Where<Xbim.Ifc2x3.Kernel.IfcProduct>(w => w.EntityLabel == instance.IfcProductLabel);
                                    globalID = element.First().GlobalId;
                                    rdfIRI = "xxxxx";
                                    for (int i = 0; i < IDs.ElementAt(0).Count(); i++)
                                    {
                                        if (IDs.ElementAt(0).ElementAt(i) == globalID)
                                        {
                                            rdfIRI = IDs.ElementAt(1).ElementAt(i);
                                            break;
                                        }
                                    }
                                    name = element.First().Name;
                                    className = element.First().GetType().Name;
                                }

                                // convert to float and int
                                List<double> verticesFloat = new List<double>();
                                foreach (XbimPoint3D point in vertices)
                                {
                                    XbimPoint3D pointTrans = point * instanceTransform;
                                    verticesFloat.Add((double)pointTrans.X * (double)meterConversion);
                                    verticesFloat.Add((double)pointTrans.Y * (double)meterConversion);
                                    verticesFloat.Add((double)pointTrans.Z * (double)meterConversion);
                                }

                                List<List<int>> facesInt = new List<List<int>>();
                                foreach (XbimFaceTriangulation face in faces)
                                {
                                    int nrOfIndices = face.Indices.Count();
                                    for (int i = 0; i < (nrOfIndices / 3); i++)
                                    {
                                        var list = new List<int>();
                                        list.Add(face.Indices.ElementAt(i * 3));
                                        list.Add(face.Indices.ElementAt(i * 3 + 1));
                                        list.Add(face.Indices.ElementAt(i * 3 + 2));
                                        facesInt.Add(list);
                                    }
                                }

                                using (var writeStream = new FileStream(outputPlyDirPath + "\\ifcproduct" + counter.ToString() + ".ply", FileMode.Create, FileAccess.Write))
                                {
                                    var writeFile = new PlyFile();

                                    writeFile.Comments.Add("generator: xbim + tinyply");
                                    writeFile.Comments.Add("class_name: " + className);
                                    writeFile.Comments.Add("given_name: " + name);
                                    writeFile.Comments.Add("ifc_ID: " + globalID);
                                    writeFile.Comments.Add("rdf_iri: " + rdfIRI);
                                    writeFile.AddPropertiesToElement("vertex", new[] { "x", "y", "z" }, verticesFloat);
                                    writeFile.AddListPropertyToElement("face", "vertex_index", facesInt, typeof(int));
                                    writeFile.Write(writeStream);

                                    counter++;
                                }
                            }
                        }
                    }
                }
            }
            // write bounding boxes to file
            using (StreamWriter writer = new StreamWriter(outputBBoxPath))
            {
                foreach (var tuple in boundingBoxes)
                {
                    string line = $"{tuple.Item1},{string.Join(", ", tuple.Item2.Select(d => d.ToString(CultureInfo.InvariantCulture)))}";
                    writer.WriteLine(line);
                }
            }
        }
    }
}
