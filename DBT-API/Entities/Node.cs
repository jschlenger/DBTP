
namespace DBT_API.Entities
{
    public class Node
    {
        public string IRI { get; set; }
        public string Domain { get; set; }
        public List<string> Classes { get; set; }
        public List<Edge> Relations { get; set; }
    }

    public class Edge
    {
        public string Name { get; set; }
        public string ObjectIRI { get; set; }
    }
}
