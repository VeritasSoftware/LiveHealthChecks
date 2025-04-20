using Microsoft.AspNetCore.Mvc;

namespace Sample.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthCheckController : ControllerBase
    {

        [HttpGet("IsAlive")]
        public IActionResult IsAlive()
        {
            return Ok("Health Check is OK");
        }
    }
}
