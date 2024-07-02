using DBT_API.Entities;
using VDS.RDF;

namespace DBT_API.Repositories
{
    public class InMemValidationRepository : IValidationRepository
    {
        private readonly List<Validation> validations = new();
        public async Task<IEnumerable<Validation>> GetValidationsAsync()
        {
            return await Task.FromResult(validations);
        }

        public async Task<Validation> GetValidationAsync(Guid id)
        {
            var validation = validations.Where(val => val.Id == id).SingleOrDefault();
            return await Task.FromResult(validation);
        }

        public async Task SetValidationAsync(Validation val)
        {
            validations.Add(val);
            await Task.CompletedTask;
        }

        public async Task DeleteValidationAsync(Guid id)
        {
            var index = validations.FindIndex(existingVals => existingVals.Id == id);
            validations.RemoveAt(index);
            await Task.CompletedTask;
        }

        public List<Graph> GetValidationGraphs()
        {
            List<Graph> validationGraphs = new();
            foreach (var validation in validations)
            {
                Graph validationGraph = new();
                validationGraph.LoadFromString(validation.ValidationString);
                validationGraphs.Add(validationGraph);
            }
            return validationGraphs;
        }
    }
}
