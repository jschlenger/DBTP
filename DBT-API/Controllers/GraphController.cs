using DBT_API.Entities;
using DBT_API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DBT_API.Controllers
{
    [ApiController]
    [Route("node")]
    public class GraphController : ControllerBase
    {
        private readonly IGraphRepository graphRepository;
        private readonly IValidationRepository validationRepository;
        private readonly ILogger<GraphController> logger;

        public GraphController(IGraphRepository graphRepository, IValidationRepository validationRepository, ILogger<GraphController> logger)
        {
            this.graphRepository = graphRepository;
            this.validationRepository = validationRepository;
            this.logger = logger;
        }

        // Post /node
        [HttpPost]
        public async Task<ActionResult<List<string>>> AddNodesAsync(List<Node> nodes)
        {
            IEnumerable<string> addedNodeIRIs = await graphRepository.AddNodesAsync(nodes, validationRepository.GetValidationGraphs());
            if (addedNodeIRIs.Count() == 0)
            {
                return BadRequest("At least one of the specified nodes is already existing.");
            }
            else if (addedNodeIRIs.Count() == 1 && addedNodeIRIs.ElementAt(0).Contains("Validation error"))
            {
                return BadRequest(addedNodeIRIs.ElementAt(0));
            }
            else
            {
                return Ok(addedNodeIRIs);
            }
        }

        // Get /node
        [HttpGet]
        public async Task<ActionResult<Node>> GetNodeAsync(string iri)
        {
            Node node = await graphRepository.GetNodeAsync(iri);
            if (node == null)
                return NotFound();
            else
                return Ok(node);
        }

        // Put /node
        [HttpPut]
        public async Task<ActionResult<List<string>>> UpdateNodesAsync(List<Node> nodes)
        {
            IEnumerable<string> updatedNodeIRIs = await graphRepository.UpdateNodesAsync(nodes, validationRepository.GetValidationGraphs());
            if (updatedNodeIRIs.Count() == 0)
            {
                return BadRequest("At least one of the specified nodes is not existing.");
            }
            else if (updatedNodeIRIs.Count() == 1 && updatedNodeIRIs.ElementAt(0).Contains("Validation error"))
            {
                return BadRequest(updatedNodeIRIs.ElementAt(0));
            }
            else
            {
                return Ok(updatedNodeIRIs);
            }
        }

        // DELETE /node/{id}
        [HttpDelete("{iri}")]
        public async Task<ActionResult> DeleteNodeAsync(string iri)
        {
            bool deleted = await graphRepository.DeleteNodeAsync(iri);
            if (!deleted)
                return NotFound();
            else
                return NoContent();
        }
    }
}
