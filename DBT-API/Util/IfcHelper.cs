using System.Runtime.InteropServices;
using GeometryReader;

namespace DBT_API.Util
{

    //public static class NativeMethods
    //{
    //    [DllImport("Xbim.Geometry.Engine64.dll", SetLastError = true)]
    //    public static extern List<Tuple<string, List<double>>> IfcToPly(string path, List<List<string>> IDs, string outputDirPath);
    //}
    public static class IfcHelper
    {
        public static List<Tuple<string, List<double>>> IfcToPly(string path, List<List<string>> IDs, string outputDirPath)
        {
            //// call function from other project since the XBIM.Geometry Nuget package is not compatible with .NET 6.0
            //List<Tuple<string, List<double>>> result = IFCGeometryReader.IFCGeometryReader.IfcToPly(path, IDs, outputDirPath);
            //return result;

            // call function from external library
            List<Tuple<string, List<double>>> result = IFCReader.ToPly(path, IDs, outputDirPath);
            return result;

            //List<Tuple<string, List<double>>> boundingBoxes = new();

            //using (var model = IfcStore.Open(path))
            //{
            //    var schemaVersion = model.SchemaVersion;

            //    double meterConversion = model.ModelFactors.LengthToMetresConversionFactor;

            //Xbim3DModelContext modelContext = new Xbim3DModelContext(model);
            //modelContext.CreateContext();
            //List<XbimShapeInstance> shapeInstances = modelContext.ShapeInstances().ToList();

            //int counter = 0;
            //foreach (var instance in shapeInstances)
            //{
            //    if (instance.RepresentationType == XbimGeometryRepresentationType.OpeningsAndAdditionsIncluded)
            //    {
            //        var instanceTransform = instance.Transformation;

            //        XbimShapeGeometry geometry = modelContext.ShapeGeometry(instance);
            //        XbimRect3D boundingBox = geometry.BoundingBox.Transform(instanceTransform);

            //        List<double> bounds = new List<double>();
            //        bounds.Add(boundingBox.Min.X * meterConversion);
            //        bounds.Add(boundingBox.Min.Y * meterConversion);
            //        bounds.Add(boundingBox.Min.Z * meterConversion);
            //        bounds.Add(boundingBox.Max.X * meterConversion);
            //        bounds.Add(boundingBox.Max.Y * meterConversion);
            //        bounds.Add(boundingBox.Max.Z * meterConversion);

            //        string ID;
            //        if (schemaVersion.ToString() == "Ifc4")
            //        {
            //            var element = model.Instances.Where<Xbim.Ifc4.Kernel.IfcProduct>(w => w.EntityLabel == instance.IfcProductLabel);
            //            ID = element.First().GlobalId;
            //        }
            //        else
            //        {
            //            var element = model.Instances.Where<Xbim.Ifc2x3.Kernel.IfcProduct>(w => w.EntityLabel == instance.IfcProductLabel);
            //            ID = element.First().GlobalId;
            //        }

            //        Tuple<string, List<double>> tuple = new Tuple<string, List<double>>(ID, bounds);
            //        boundingBoxes.Add(tuple);

            //        byte[] data = ((IXbimShapeGeometryData)geometry).ShapeData;

            //        using (var stream = new MemoryStream(data))
            //        {
            //            using (var reader = new BinaryReader(stream))
            //            {
            //                var productMesh = reader.ReadShapeTriangulation();

            //                List<XbimFaceTriangulation> faces = productMesh.Faces as List<XbimFaceTriangulation>;
            //                List<XbimPoint3D> vertices = productMesh.Vertices as List<XbimPoint3D>;

            //                // get globalID and class name of the object the geometry belongs to
            //                string className;
            //                string name;
            //                string globalID;
            //                string rdfIRI;
            //                if (schemaVersion.ToString() == "Ifc4")
            //                {
            //                    var element = model.Instances.Where<Xbim.Ifc4.Kernel.IfcProduct>(w => w.EntityLabel == instance.IfcProductLabel);
            //                    globalID = element.First().GlobalId;
            //                    rdfIRI = "xxxxx";
            //                    for (int i = 0; i < IDs.ElementAt(0).Count(); i++)
            //                    {
            //                        if (IDs.ElementAt(0).ElementAt(i) == globalID)
            //                        {
            //                            rdfIRI = IDs.ElementAt(1).ElementAt(i);
            //                            break;
            //                        }
            //                    }
            //                    name = element.First().Name;
            //                    className = element.First().GetType().Name;
            //                }
            //                else
            //                {
            //                    var element = model.Instances.Where<Xbim.Ifc2x3.Kernel.IfcProduct>(w => w.EntityLabel == instance.IfcProductLabel);
            //                    globalID = element.First().GlobalId;
            //                    rdfIRI = "xxxxx";
            //                    for (int i = 0; i < IDs.ElementAt(0).Count(); i++)
            //                    {
            //                        if (IDs.ElementAt(0).ElementAt(i) == globalID)
            //                        {
            //                            rdfIRI = IDs.ElementAt(1).ElementAt(i);
            //                            break;
            //                        }
            //                    }
            //                    name = element.First().Name;
            //                    className = element.First().GetType().Name;
            //                }

            //                // convert to float and int
            //                List<double> verticesFloat = new List<double>();
            //                foreach (XbimPoint3D point in vertices)
            //                {
            //                    XbimPoint3D pointTrans = point * instanceTransform;
            //                    verticesFloat.Add((double)pointTrans.X * (double)meterConversion);
            //                    verticesFloat.Add((double)pointTrans.Y * (double)meterConversion);
            //                    verticesFloat.Add((double)pointTrans.Z * (double)meterConversion);
            //                }

            //                List<List<int>> facesInt = new List<List<int>>();
            //                foreach (XbimFaceTriangulation face in faces)
            //                {
            //                    int nrOfIndices = face.Indices.Count();
            //                    for (int i = 0; i < (nrOfIndices / 3); i++)
            //                    {
            //                        var list = new List<int>();
            //                        list.Add(face.Indices.ElementAt(i * 3));
            //                        list.Add(face.Indices.ElementAt(i * 3 + 1));
            //                        list.Add(face.Indices.ElementAt(i * 3 + 2));
            //                        facesInt.Add(list);
            //                    }
            //                }

            //                using (var writeStream = new FileStream(outputDirPath + "\\ifcproduct" + counter.ToString() + ".ply", FileMode.Create, FileAccess.Write))
            //                {
            //                    var writeFile = new PlyFile();

            //                    writeFile.Comments.Add("generator: xbim + tinyply");
            //                    writeFile.Comments.Add("class_name: " + className);
            //                    writeFile.Comments.Add("given_name: " + name);
            //                    writeFile.Comments.Add("ifc_ID: " + globalID);
            //                    writeFile.Comments.Add("rdf_iri: " + rdfIRI);
            //                    writeFile.AddPropertiesToElement("vertex", new[] { "x", "y", "z" }, verticesFloat);
            //                    writeFile.AddListPropertyToElement("face", "vertex_index", facesInt, typeof(int));
            //                    writeFile.Write(writeStream);

            //                    counter++;
            //                }
            //            }
            //        }
            //    }
            //}
            //}
            //return boundingBoxes;
        }

        //public static List<string> ReadIfcIds(string path)
        //{
        //    List<string> ifcIds = new List<string>();
        //    using (var model = IfcStore.Open(path))
        //    {
        //        var schemaVersion = model.SchemaVersion;

        //        Xbim3DModelContext modelContext = new Xbim3DModelContext(model);
        //        modelContext.CreateContext();

        //        List<XbimShapeInstance> shapeInstances = modelContext.ShapeInstances().ToList();

        //        foreach (var instance in shapeInstances)
        //        {
        //            if (instance.RepresentationType == XbimGeometryRepresentationType.OpeningsAndAdditionsIncluded)
        //            {
        //                string name;
        //                if (schemaVersion.ToString() == "Ifc4")
        //                {
        //                    var element = model.Instances.Where<Xbim.Ifc4.Kernel.IfcProduct>(w => w.EntityLabel == instance.IfcProductLabel);
        //                    name = element.First().Name;
        //                }
        //                else
        //                {
        //                    var element = model.Instances.Where<Xbim.Ifc2x3.Kernel.IfcProduct>(w => w.EntityLabel == instance.IfcProductLabel);
        //                    name = element.First().Name;
        //                }
        //                ifcIds.Add(name);
        //            }
        //        }
        //    }

        //    return ifcIds;
        //}
    }
}
