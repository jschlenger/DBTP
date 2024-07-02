
namespace DBT_API.Entities
{
    public class Blob
    {
        public string Etag { get; set; }
        public string Bucket { get; set; }
        public string FileName { get; set; }
        public IEnumerable<string> NodeIris { get; set; }
    }
}
