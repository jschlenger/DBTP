using DBT_API.Entities;
using VDS.RDF;

namespace DBT_API.Repositories
{
    public interface IGraphRepository
    {
        Task<IEnumerable<string>> AddNodesAsync(List<Node> nodes, List<Graph> validationGraphs);
        Task<Node> GetNodeAsync(string IRI);
        Task<IEnumerable<string>> UpdateNodesAsync(List<Node> nodes, List<Graph> validationGraphs);
        Task<bool> DeleteNodeAsync(string IRI);
    }
}
