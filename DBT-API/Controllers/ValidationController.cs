using DBT_API.Dtos;
using DBT_API.Entities;
using DBT_API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DBT_API.Controllers
{
    [ApiController]
    [Route("validation")]
    public class ValidationController : ControllerBase
    {
        private readonly IValidationRepository valRepository;
        private readonly ILogger<ValidationController> logger;

        public ValidationController(IValidationRepository valRepository, ILogger<ValidationController> logger)
        {
            this.valRepository = valRepository;
            this.logger = logger;
        }

        // Get /valiation/
        [HttpGet]
        public async Task<IEnumerable<Guid>> GetValidationsAsync()
        {
            var validations = (await valRepository.GetValidationsAsync())
                .Select(val => val.AsDto());

            return validations.Select(vali => vali.Id).ToList();
        }

        // Get /valiation/
        [HttpGet("{id}")]
        public async Task<ActionResult<ValidationDto>> GetValidationAsync(Guid id)
        {
            var validation = await valRepository.GetValidationAsync(id);

            if (validation is null)
            {
                return NotFound();
            }

            return validation.AsDto();
        }

        // Post /validation/
        [HttpPost]
        public async Task<ActionResult<ValidationDto>> SetValidationAsync(CreateValidationDto validationDto)
        {
            Validation val = new()
            {
                Id = Guid.NewGuid(),
                Name = validationDto.Name,
                Description = validationDto.Description,
                ValidationString = validationDto.ValidationString
            };

            await valRepository.SetValidationAsync(val);

            return CreatedAtAction(nameof(GetValidationAsync), new { id = val.Id }, val.AsDto());
        }

        // Delete /validation/
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteValidationAsync(Guid id)
        {
            var existingValidation = await valRepository.GetValidationAsync(id);

            if (existingValidation is null)
            {
                return NotFound();
            }

            await valRepository.DeleteValidationAsync(id);

            return NoContent();
        }
    }
}

