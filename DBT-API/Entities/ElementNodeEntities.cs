using Newtonsoft.Json;

namespace DBT_API.Entities
{
    public class Storey : Zone
    {
        [JsonProperty(PropertyName = "http://lbd.arch.rwth-aachen.de/props#elevationIfcBuildingStorey_attribute_simple")]
        public double _elevationIfcBuildingStorey_attribute_simple { get; set; }
    }

    public class Site : Zone
    {
        [JsonProperty(PropertyName = "http://lbd.arch.rwth-aachen.de/props#refElevationIfcSite_attribute_simple")]
        public double _refElevationIfcSite_attribute_simple { get; set; }
    }

    public class Element : Thing
    {
        [JsonProperty(PropertyName = "http://lbd.arch.rwth-aachen.de/props#globalIdIfcRoot_attribute_simple")]
        public string _globalIdIfcRoot_attribute_simple { get; set; }

        [JsonProperty(PropertyName = "http://lbd.arch.rwth-aachen.de/props#nameIfcRoot_attribute_simple")]
        public string _nameIfcRoot_attribute_simple { get; set; }

        [JsonProperty(PropertyName = "http://lbd.arch.rwth-aachen.de/props#objectTypeIfcObject_attribute_simple")]
        public string _objectTypeIfcObject_attribute_simple { get; set; }

        [JsonProperty(PropertyName = "http://lbd.arch.rwth-aachen.de/props#batid_attribute_simple")]
        public string _batid_attribute_simple { get; set; }

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

    public class Zone : Thing
    {
        [JsonProperty(PropertyName = "http://lbd.arch.rwth-aachen.de/props#globalIdIfcRoot_attribute_simple")]
        public string _globalIdIfcRoot_attribute_simple { get; set; }

        [JsonProperty(PropertyName = "http://lbd.arch.rwth-aachen.de/props#nameIfcRoot_attribute_simple")]
        public string _nameIfcRoot_attribute_simple { get; set; }

        [JsonProperty(PropertyName = "http://lbd.arch.rwth-aachen.de/props#objectTypeIfcObject_attribute_simple")]
        public string _objectTypeIfcObject_attribute_simple { get; set; }

        [JsonProperty(PropertyName = "http://lbd.arch.rwth-aachen.de/props#longNameIfcSpatialStructureElement_attribute_simple")]
        public string _longNameIfcSpatialStructureElement_attribute_simple { get; set; }

        [JsonProperty(PropertyName = "http://lbd.arch.rwth-aachen.de/props#longNameIfcSpatialElement_attribute_simple")]
        public string _longNameIfcSpatialElement_attribute_simple { get; set; }
    }

    public class Building : Zone
    {

    }

    public class Interface : Thing
    {

    }

    public class Space : Zone
    {

    }

    public class Slab : Element
    {

    }

    public class Column : Element
    {

    }

    public class Wall : Element
    {

    }

    public class Pile : Element
    {

    }

    public class StairFlight : Element
    {

    }

    public class Member : Element
    {

    }

    public class ShadingDevice : Element
    {

    }

    public class Stair : Element
    {

    }

    public class Beam : Element
    {

    }

    public class Footing : Element
    {

    }

    public class Door : Element
    {

    }

    public class Railing : Element
    {

    }

    public class RampFlight : Element
    {

    }

    public class Ramp : Element
    {

    }

    public class Plate : Element
    {

    }

    public class Roof : Element
    {

    }

    public class Covering : Element
    {

    }

    public class Window : Element
    {

    }
}
