using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PredictionController : ControllerBase
    {
        [HttpPost]
        public IActionResult Predict()
        {
            return Ok(new { Message = "prediction received successfully!" });
        }
    }
}
