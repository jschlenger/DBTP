using Newtonsoft.Json;

namespace DBT_API.Entities
{
    public class Thing : Node
    {
        [JsonProperty(PropertyName = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#id")]
        public int _id { get; set; }
        [JsonProperty(PropertyName = "http://iot.linkeddata.es/def/wot#name")]
        public string _name { get; set; }
        [JsonProperty(PropertyName = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#classificationCode")]
        public string _classificationCode { get; set; }
        [JsonProperty(PropertyName = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#classificationSystem")]
        public string _classificationSystem { get; set; }
    }

    public class AsPlannedResource : Resource
    {

        [JsonProperty(PropertyName = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#contractedEnd")]
        public DateTime _contractedEnd { get; set; }

        [JsonProperty(PropertyName = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#contractedStart")]
        public DateTime _contractedStart { get; set; }

    }

    public class AsPlannedTemporaryEquipment : AsPlannedResource
    {

    }

    public class VolumetricDefect : GeometricDefect
    {

    }

    public class ProcessPrecondition : Precondition
    {

    }

    public class AsPlannedWorker : AsPlannedResource
    {

    }

    public class AsPlannedWorkerCrew : AsPlannedResource
    {

    }

    public class WorkingZone : Thing
    {

    }

    public class AsPlannedWorkingZone : WorkingZone
    {

    }

    public class ZonePrecondition : Precondition
    {

        [JsonProperty(PropertyName = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#availableFrom")]
        public DateTime _availableFrom { get; set; }

        [JsonProperty(PropertyName = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#availableTill")]
        public DateTime _availableTill { get; set; }

    }

    public class ConstructionSchedule : Thing
    {

        [JsonProperty(PropertyName = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#baselinePlanFrom")]
        public DateTime _baselinePlanFrom { get; set; }

        [JsonProperty(PropertyName = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#baselinePlanTill")]
        public DateTime _baselinePlanTill { get; set; }

    }

    public class Defect : Thing
    {

        [JsonProperty(PropertyName = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#criticality")]
        public int _criticality { get; set; }

        [JsonProperty(PropertyName = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#timeStamp")]
        public DateTime _timeStamp { get; set; }

    }

    public class ExternalFactorPrecondition : Precondition
    {

        [JsonProperty(PropertyName = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#thresholdValue")]
        public double _thresholdValue { get; set; }

    }

    public class GeometricDefect : Defect
    {

    }

    public class InformationPrecondition : Precondition
    {

    }

    public class LocationBreakdownStructure : Thing
    {

    }

    public class AsPerformedEquipment : AsPerformedResource
    {

    }

    public class PositionDefect : GeometricDefect
    {

    }

    public class AsPerformedMaterial : AsPerformedResource
    {

    }

    public class Precondition : Thing
    {

        [JsonProperty(PropertyName = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#fulfilled")]
        public bool _fulfilled { get; set; }

    }

    public class AsPerformedResource : Resource
    {

        [JsonProperty(PropertyName = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#contractedEnd")]
        public DateTime _contractedEnd { get; set; }

        [JsonProperty(PropertyName = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#contractedStart")]
        public DateTime _contractedStart { get; set; }

    }

    public class SpatialObject : Thing
    {

    }

    public class Geometry : SpatialObject
    {
        [JsonProperty(PropertyName = "http://www.opengis.net/ont/geosparql#asWKT")]
        public string _asWKT { get; set; }

        [JsonProperty(PropertyName = "https://w3id.org/fog#asPly")]
        public string _asPly { get; set; }

        [JsonProperty(PropertyName = "https://w3id.org/fog#asIfc")]
        public string _asIfc { get; set; }

        [JsonProperty(PropertyName = "https://w3id.org/fog#asGltf")]
        public string _asGltf { get; set; }

        [JsonProperty(PropertyName = "https://w3id.org/fog#asStl")]
        public string _asStl { get; set; }

        [JsonProperty(PropertyName = "https://w3id.org/fog#asObj")]
        public string _asObj { get; set; }

        [JsonProperty(PropertyName = "https://w3id.org/fog#asE57")]
        public string _asE57 { get; set; }

        [JsonProperty(PropertyName = "https://w3id.org/fog#asPcd")]
        public string _asPcd { get; set; }
    }

    public class Process : Thing
    {

        [JsonProperty(PropertyName = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#contractor")]
        public string _contractor { get; set; }

        [JsonProperty(PropertyName = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#endTime")]
        public DateTime _endTime { get; set; }

        [JsonProperty(PropertyName = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#startTime")]
        public DateTime _startTime { get; set; }

    }

    public class AsPerformedTemporaryEquipment : AsPerformedResource
    {

    }

    public class AsPerformedWorker : AsPerformedResource
    {

    }

    public class Resource : Thing
    {

        [JsonProperty(PropertyName = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#contractor")]
        public string _contractor { get; set; }

        [JsonProperty(PropertyName = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#cost")]
        public double _cost { get; set; }

        [JsonProperty(PropertyName = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#quantity")]
        public double _quantity { get; set; }

    }

    public class AsPerformedWorkerCrew : AsPerformedResource
    {

    }

    public class AsPerformedWorkingZone : WorkingZone
    {

    }

    public class ResourceApplication : Thing
    {

        [JsonProperty(PropertyName = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#endTime")]
        public DateTime _endTime { get; set; }

        [JsonProperty(PropertyName = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#quantity")]
        public double _quantity { get; set; }

        [JsonProperty(PropertyName = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#startTime")]
        public DateTime _startTime { get; set; }

        [JsonProperty(PropertyName = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#utilizationRate")]
        public double _utilizationRate { get; set; }

    }

    public class ResourceAssignment : Precondition
    {

        [JsonProperty(PropertyName = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#endTime")]
        public DateTime _endTime { get; set; }

        [JsonProperty(PropertyName = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#quantity")]
        public double _quantity { get; set; }

        [JsonProperty(PropertyName = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#startTime")]
        public DateTime _startTime { get; set; }

        [JsonProperty(PropertyName = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#utilizationRate")]
        public double _utilizationRate { get; set; }

    }

    public class AsPlannedEquipment : AsPlannedResource
    {

    }

    public class SurfaceDefect : GeometricDefect
    {

    }

    public class AsPlannedMaterial : AsPlannedResource
    {

    }

    public class AsPerformedProcess : Process
    {

        [JsonProperty(PropertyName = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#timeStamp")]
        public DateTime _timeStamp { get; set; }

    }

    public class AsPlannedProcess : Process
    {

    }

    public class ProcessDecomposition : Thing
    {

        [JsonProperty(PropertyName = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#decompositionLevel")]
        public int _decompositionLevel { get; set; }

    }

    public class DecompositionCriterium : Thing
    {

    }

    public static class SequenceType
    {

        public static readonly string EndEnd = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#EndEnd";
        public static readonly string EndStart = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#EndStart";
        public static readonly string StartEnd = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#StartEnd";
        public static readonly string StartStart = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#StartStart";

    }

    public static class DefectStatusType
    {

        public static readonly string Ongoing = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#Ongoing";
        public static readonly string Resolved = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#Resolved";

    }

    public static class GeometryStatusType
    {

        public static readonly string CompletelyDetected = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#CompletelyDetected";
        public static readonly string NotDetected = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#NotDetected";
        public static readonly string PartiallyDetected = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#PartiallyDetected";

    }

    public static class StatusType
    {

        public static readonly string Finished = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#Finished";
        public static readonly string InWork = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#InWork";
        public static readonly string NotStarted = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#NotStarted";
        public static readonly string Paused = "https://dtc-ontology.cms.ed.tum.de/ontology/v2#Paused";

    }
}
