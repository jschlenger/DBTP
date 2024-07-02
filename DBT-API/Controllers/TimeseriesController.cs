using DBT_API.Repositories;
using Microsoft.AspNetCore.Mvc;
using DBT_API.Dtos;
using DBT_API.Entities;
using Microsoft.Office.Interop.Excel;
using System;


namespace DBT_API.Controllers
{
    [ApiController]
    [Route("timeseries")]
    public class TimeseriesController : ControllerBase
    {
        private readonly ILogger<TimeseriesController> logger;
        private readonly ITimeseriesRepository timeseriesRepository;

        public TimeseriesController(ITimeseriesRepository timeseriesRepository, ILogger<TimeseriesController> logger)
        {
            this.logger = logger;
            this.timeseriesRepository = timeseriesRepository;
        }

        // Post /timeseries/
        [HttpPost]
        public async Task<ActionResult<IEnumerable<TimeseriesDto>>> GetTimeseriesAsync(GetTimeseriesDto timeseriesDto)
        {
            var timeseries = await timeseriesRepository.GetTimeseriesAsync(timeseriesDto);
            
            if (timeseries is null)
            {
                return NotFound();
            }

            return timeseries.Select(t => t.AsDto()).ToList();
        }

        // Post /timeseries/addbybucket
        [HttpPost("addbybucket")]
        public async Task<ActionResult> AddTimeseriesByBucketAsync(List<CreateTimeseriesByBucketDto> timeseriesDtos)
        {
            bool created = await timeseriesRepository.AddTimeseriesByBucketAsync(timeseriesDtos);
            if (!created)
                return BadRequest();
            else
                return NoContent();
        }

        // Post /timeseries/addbynodeIRI
        [HttpPost("addbynodeIRI")]
        public async Task<ActionResult> AddTimeseriesByNodeAsync(List<CreateTimeseriesByNodeDto> timeseriesDtos)
        {
            bool created = await timeseriesRepository.AddTimeseriesByNodeAsync(timeseriesDtos);
            if (!created)
                return BadRequest();
            else
                return NoContent();
        }

        // Post /timeseries/connection
        [HttpPost("connection")]
        public async Task<ActionResult> AddTimeseriesConnectionAsync(CreateTimeseriesConnectionDto connectionDto)
        {
            bool updated = await timeseriesRepository.AddTimeseriesConnectionAsync(connectionDto);
            if (!updated)
                return NotFound();
            else
                return NoContent();
        }
    }
}
