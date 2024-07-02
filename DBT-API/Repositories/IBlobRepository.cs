using DBT_API.Entities;

namespace DBT_API.Repositories
{
    public interface IBlobRepository
    {
        Task<string> AddBlobAsync(Blob blob, IFormFile file);
        Task<string> GetBlobByNameAsync(string bucket, string fileName);
        Task<string> GetBlobByNodeAsync(string nodeIRI);
        Task<bool> DeleteBlobByNameAsync(string bucket, string name);
        Task<bool> DeleteBlobByNodeAsync(string nodeIRI);
        Task<bool> ConnectBlobAsync(string bucket, string fileName, string nodeIRI);
        Task GetBlobsAsync(Guid id);
    }
}
