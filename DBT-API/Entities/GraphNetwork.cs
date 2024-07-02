namespace DBT_API.Entities
{
    public class GraphNetwork
    {
        public List<Node> Data {  get; set; }
        public List<Node> Information { get; set; }
        public List<Node> Knowledge { get; set; }

        public GraphNetwork(List<Node> data, List<Node> information, List<Node> knowledge)
        {
            this.Data = data;
            this.Information = information;
            this.Knowledge = knowledge;
        }

    }
}
