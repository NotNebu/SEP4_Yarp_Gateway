using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;

namespace ApiGateway.Yarp.Controllers.Mal
{
    [ApiController]
    [Route("api/mal")]
    public class MalController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public MalController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("MalAPI");
        }

        [HttpGet("sensors")]
        public async Task<IActionResult> GetSensorData()
        {
            var response = await _httpClient.GetAsync("/api/sensor");
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        [HttpGet("model")]
        public async Task<IActionResult> GetModel()
        {
            var response = await _httpClient.GetAsync("/api/sensor/model");
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        [HttpPost("sensors")]
        public async Task<IActionResult> PostSensorData([FromBody] object data)
        {
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/sensor", content);
            var result = await response.Content.ReadAsStringAsync();

            return StatusCode((int)response.StatusCode, result);
        }
    }
}