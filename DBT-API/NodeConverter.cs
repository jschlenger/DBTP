using DBT_API.Entities;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DBT_API
{
    public class NodeConverter : JsonConverter<Node>
    {
        public override Node Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
            {
                JsonElement root = doc.RootElement;
                JsonElement classesElement = root.GetProperty("classes");

                // Process classesElement to determine the type
                string type = DetermineTypeFromClasses(classesElement);

                Node node = type switch
                {
                    "Storey" => new Storey(),
                    "Site" => new Site(),
                    "Element" => new Element(),
                    "Zone" => new Zone(),
                    "Building" => new Building(),
                    "Interface" => new Interface(),
                    "Space" => new Space(),
                    "Slab" => new Slab(),
                    "Column" => new Column(),
                    "Wall" => new Wall(),
                    "Pile" => new Pile(),
                    "StairFlight" => new StairFlight(),
                    "Member" => new Member(),
                    "ShadingDevice" => new ShadingDevice(),
                    "Stair" => new Stair(),
                    "Beam" => new Beam(),
                    "Footing" => new Footing(),
                    "Door" => new Door(),
                    "Railing" => new Railing(),
                    "RampFlight" => new RampFlight(),
                    "Ramp" => new Ramp(),
                    "Plate" => new Plate(),
                    "Roof" => new Roof(),
                    "Covering" => new Covering(),
                    "Window" => new Window(),
                    "AsPlannedResource" => new AsPlannedResource(),
                    "AsPlannedTemporaryEquipment" => new AsPlannedTemporaryEquipment(),
                    "VolumetricDefect" => new VolumetricDefect(),
                    "ProcessPrecondition" => new ProcessPrecondition(),
                    "AsPlannedWorker" => new AsPlannedWorker(),
                    "AsPlannedWorkerCrew" => new AsPlannedWorkerCrew(),
                    "WorkingZone" => new WorkingZone(),
                    "AsPlannedWorkingZone" => new AsPlannedWorkingZone(),
                    "ZonePrecondition" => new ZonePrecondition(),
                    "ConstructionSchedule" => new ConstructionSchedule(),
                    "Defect" => new Defect(),
                    "ExternalFactorPrecondition" => new ExternalFactorPrecondition(),
                    "GeometricDefect" => new GeometricDefect(),
                    "InformationPrecondition" => new InformationPrecondition(),
                    "LocationBreakdownStructure" => new LocationBreakdownStructure(),
                    "AsPerformedEquipment" => new AsPerformedEquipment(),
                    "PositionDefect" => new PositionDefect(),
                    "AsPerformedMaterial" => new AsPerformedMaterial(),
                    "Precondition" => new Precondition(),
                    "AsPerformedResource" => new AsPerformedResource(),
                    "SpatialObject" => new SpatialObject(),
                    "Geometry" => new Geometry(),
                    "Process" => new Process(),
                    "AsPerformedTemporaryEquipment" => new AsPerformedTemporaryEquipment(),
                    "AsPerformedWorker" => new AsPerformedWorker(),
                    "Resource" => new Resource(),
                    "AsPerformedWorkerCrew" => new AsPerformedWorkerCrew(),
                    "AsPerformedWorkingZone" => new AsPerformedWorkingZone(),
                    "ResourceApplication" => new ResourceApplication(),
                    "ResourceAssignment" => new ResourceAssignment(),
                    "AsPlannedEquipment" => new AsPlannedEquipment(),
                    "SurfaceDefect" => new SurfaceDefect(),
                    "AsPlannedMaterial" => new AsPlannedMaterial(),
                    "KeyPerformanceIndicator" => new KeyPerformanceIndicator(),
                    "KeyPerformanceIndicatorAssessment" => new KeyPerformanceIndicatorAssessment(),
                    "DataService" => new DataService(),
                    "Observation" => new Observation(),
                    "Distribution" => new Distribution(),
                    "AsPlannedProcess" => new AsPlannedProcess(),
                    "AsPerformedProcess" => new AsPerformedProcess(),
                    "DecompositionCriterium" => new DecompositionCriterium(),
                    "new ProcessDecomposition()" => new ProcessDecomposition(),
                    _ => new Node(),
                };

                // Deserialize other properties
                var json = root.GetRawText();
                node = JsonSerializer.Deserialize(json, node.GetType(), options) as Node;

                return node;
            }
        }

        public override void Write(Utf8JsonWriter writer, Node value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            // Write type property based on the actual type of the object
            writer.WriteString("Type", value.GetType().Name);

            // Write other properties
            JsonSerializer.Serialize(writer, value, options);

            writer.WriteEndObject();
        }

        private string DetermineTypeFromClasses(JsonElement classesElement)
        {
            // Determine the class of the node
            string className = "";
            string lastClass = classesElement.ToString().Split("http").Last();

            int hashIndex = lastClass.IndexOf('#');
            if (hashIndex != -1)
            {
                className = lastClass.Substring(hashIndex + 1);
                className = className.Split("\"")[0];
            }
            else
            {
                int slashIndex = lastClass.LastIndexOf('/');
                if (slashIndex != -1)
                {
                    className = lastClass.Substring(slashIndex + 1);
                    className = className.Split("\"")[0];
                }
            }
            return className;
        }
    }
}
