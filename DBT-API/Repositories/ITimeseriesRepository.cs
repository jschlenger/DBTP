using DBT_API.Dtos;
using DBT_API.Entities;

namespace DBT_API.Repositories
{
    public interface ITimeseriesRepository
    {
        Task<IEnumerable<Timeseries>> GetTimeseriesAsync(GetTimeseriesDto timeseries);
        Task<bool> AddTimeseriesByBucketAsync(List<CreateTimeseriesByBucketDto> timeseriesDtos);
        Task<bool> AddTimeseriesByNodeAsync(List<CreateTimeseriesByNodeDto> timeseriesDtos);
        Task<bool> AddTimeseriesConnectionAsync(CreateTimeseriesConnectionDto connectionDto);
    }
}