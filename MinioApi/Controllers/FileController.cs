using Microsoft.AspNetCore.Mvc;

namespace MinioApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {

        [HttpPost]
        public async Task UploadStream([FromForm] IFormFile file)
        {

        }
    }
}
