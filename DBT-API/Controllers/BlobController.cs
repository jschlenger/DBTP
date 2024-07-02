using DBT_API.Dtos;
using DBT_API.Entities;
using DBT_API.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Office.Interop.Excel;


namespace DBT_API.Controllers
{
    [ApiController]
    [Route("blob")]
    public class BlobController : ControllerBase
    {
        private readonly IBlobRepository blobRepository;
        private readonly ILogger<BlobController> logger;

        public BlobController(IBlobRepository blobRepository, ILogger<BlobController> logger)
        {
            this.blobRepository = blobRepository;
            this.logger = logger;
        }

        // Post /blob
        [HttpPost]
        public async Task<ActionResult<BlobDto>> AddBlobAsync(IFormFile file, [FromForm]CreateBlobDto blobdto)
        {
            if (file == null)
            {
                return BadRequest("Please provide a single file.");
            }

            Blob blob = new()
            {
                Etag = "-",
                Bucket = blobdto.Bucket,
                FileName = file.FileName,
                NodeIris = blobdto.NodeIris
            };

            var creation = await blobRepository.AddBlobAsync(blob, file);
            var stringSplit = creation.Split('/');
            blob.Etag = stringSplit[1];
            blob.FileName = stringSplit[0];

            return Ok(blob);
        }

        // Get /blob/byname
        [HttpGet("byname")]
        public async Task<FileStreamResult> GetBlobByNameAsync(string fileName)
        {
            var splitString = fileName.Split('/');
            string bucket = splitString[0];
            string name = splitString[1];

            var tmpFile = await blobRepository.GetBlobByNameAsync(bucket, name);
            if (tmpFile == null)
            {
                string empty = null;
                return new FileStreamResult(null, empty);
            }
            else
            {
                var stream = System.IO.File.OpenRead(tmpFile);
                return new FileStreamResult(stream, "application/octet-stream");
            }
            
        }

        // Get /blob/bynode
        [HttpGet("bynode")]
        public async Task<FileStreamResult> GetBlobByNodeAsync(string nodeIRI)
        {
            var tmpFile = await blobRepository.GetBlobByNodeAsync(nodeIRI);
            if (tmpFile == null)
            {
                string empty = null;
                return new FileStreamResult(null, empty);
            }
            else
            {
                var stream = System.IO.File.OpenRead(tmpFile);
                return new FileStreamResult(stream, "application/octet-stream");
            }

        }

        // DELETE /blob/byname
        [HttpDelete("byname")]
        public async Task<ActionResult> DeleteBlobByNameAsync(string name)
        {
            var splitString = name.Split('/');
            string bucket = splitString[0];
            string fileName = splitString[1];
            bool deleted = await blobRepository.DeleteBlobByNameAsync(bucket, fileName);
            if (deleted)
                return NoContent();
            else
                return NotFound();
        }

        // DELETE /blob/bynode
        [HttpDelete("bynode")]
        public async Task<ActionResult> DeleteBlobByNodeAsync(string nodeIRI)
        {
            bool deleted = await blobRepository.DeleteBlobByNodeAsync(nodeIRI);
            if (deleted)
                return NoContent();
            else
                return NotFound();
        }

        // POST /blob/connect/
        [HttpPost("connect")]
        public async Task<ActionResult> ConnectBlobAsync(string filename, string nodeIRI)
        {
            var splitString = filename.Split('/');
            string bucket = splitString[0];
            string fileName = splitString[1];
            bool connected = await blobRepository.ConnectBlobAsync(bucket, fileName, nodeIRI);
            if (connected)
                return NoContent();
            else
                return NotFound();
        }
    }
}
