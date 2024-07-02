
namespace DBT_API.Repositories
{
    public interface ISetupRepository
    {
        Task SetBuildingOneAsync(string outputFilePath);
        Task SetBuildingTwoAsync(string IDFilePath, string BBoxFilePath);
        Task SetScheduleAsync(string scheduleFilePath, string linksFilePath);
    }
}
