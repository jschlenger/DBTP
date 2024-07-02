using Microsoft.AspNetCore.Mvc;
using DBT_API.Repositories;

namespace DBT_API.Controllers
{
    [ApiController]
    [Route("setup")]
    public class PIIController : ControllerBase
    {
        private readonly ILogger<PIIController> logger;
        private readonly ISetupRepository setupRepository;

        public PIIController(ISetupRepository setupRepository, ILogger<PIIController> logger)
        {
            this.setupRepository = setupRepository;
            this.logger = logger;
        }

        // Post /setup/setbuildingpart1
        [HttpPost("setbuildingpart1")]
        public async Task<ActionResult> SetBuildingOneAsync(string outputFilePath)
        {
            await setupRepository.SetBuildingOneAsync(outputFilePath);
            return Ok();
        }

        // Post /setup/setbuildingpart2
        [HttpPost("setbuildingpart2")]
        public async Task<ActionResult> SetBuildingTwoAsync(string IDFilePath, string BBoxFilePath)
        {
            await setupRepository.SetBuildingTwoAsync(IDFilePath, BBoxFilePath);
            return Ok();
        }

        // Post /setup/setschedule
        [HttpPost("setschedule")]
        public async Task<ActionResult> SetScheduleAsync(string scheduleFilePath, string linksFilePath)
        {
            await setupRepository.SetScheduleAsync(scheduleFilePath, linksFilePath);
            return Ok();
        }
    }
}

