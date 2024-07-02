using Newtonsoft.Json;

namespace DBT_API.Entities
{
    public class KeyPerformanceIndicator : Thing
    { 
        [JsonProperty(PropertyName = "https://saref.etsi.org/core/hasName")]
        public string _hasName { get; set; }

        [JsonProperty(PropertyName = "https://saref.etsi.org/core/hasDescription")]
        public string _hasDescription { get; set; }

    }

    public class KeyPerformanceIndicatorAssessment : Thing 
    {
        [JsonProperty(PropertyName = "https://saref.etsi.org/core/hasName")]
        public string _hasName { get; set; }

        [JsonProperty(PropertyName = "https://saref.etsi.org/core/hasDescription")]
        public string _hasDescription { get; set; }

        [JsonProperty(PropertyName = "https://saref.etsi.org/core/hasValue")]
        public double _hasValue { get; set; }

        [JsonProperty(PropertyName = "https://saref.etsi.org/saref4city/hasLastUpdateDate")]
        public DateTime _hasLastUpdateDate { get; set; }

        [JsonProperty(PropertyName = "https://saref.etsi.org/saref4city/hasCreationDate")]
        public DateTime _hasCreationDate { get; set; }

        [JsonProperty(PropertyName = "https://saref.etsi.org/saref4city/hasExpirationDate")]
        public DateTime _hasExpirationDate { get; set; }
    }

    public class DataService : Thing
    {
        [JsonProperty(PropertyName = "http://www.w3.org/ns/dcat#endpointUrl")]
        public string _endpointUrl { get; set; }

        [JsonProperty(PropertyName = "http://purl.org/dc/terms/identifier")]
        public string _identifier { get; set; }

        [JsonProperty(PropertyName = "http://www.w3.org/ns/dcat#endpointDescription")]
        public string _endpointDescription { get; set; }
    }

    public class  Dataset : Thing
    {
        
    }

    public class Observation : Thing
    {

    }

    public class Distribution : Thing
    {
        [JsonProperty(PropertyName = "http://www.w3.org/ns/dcat#downloadUrl")]
        public string _downloadUrl { get; set; }
    }
}
