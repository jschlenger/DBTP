using DBT_API.Entities;
using VDS.RDF;


namespace DBT_API.Repositories
{
    public interface IValidationRepository
    {
        Task<IEnumerable<Validation>> GetValidationsAsync();
        Task<Validation> GetValidationAsync(Guid id);
        Task SetValidationAsync(Validation val);
        Task DeleteValidationAsync(Guid id);
        List<Graph> GetValidationGraphs();
    }
}
