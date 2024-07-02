
namespace DBT_API.Entities
{
    public class Timeseries
    {
        public string NodeIRI { get; set; }
        public DateTime Time { get; set; }
        public double Value { get; set; }
        public List<Tuple<string, string>> Tags { get; set; }
    }
}