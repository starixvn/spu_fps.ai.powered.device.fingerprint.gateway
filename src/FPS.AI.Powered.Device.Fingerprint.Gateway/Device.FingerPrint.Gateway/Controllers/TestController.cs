using Microsoft.AspNetCore.Mvc;

namespace Device.FingerPrint.Gateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { 
                message = "Gateway is working!", 
                timestamp = DateTime.UtcNow,
                routes = new[] {
                    "/FPS/CheckTransaction -> localhost:5250/FPS/InsertTransaction",
                    "/FPS/MobileCheckTransaction -> localhost:5250/FPS/InsertMobileTransaction"
                }
            });
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { 
                status = "healthy", 
                service = "FPS Gateway API",
                version = "1.0.0"
            });
        }
    }
}
