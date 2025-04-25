using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using APII;
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
        public async Task<IActionResult> PostSensorData([FromBody] PostSensorData data)
        {
            try
            {
                // Serialize the sensor data object into JSON
                var json = JsonConvert.SerializeObject(data);

                // Create the content with the serialized JSON
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Make the POST request to the external sensor API
                var response = await _httpClient.PostAsync("/api/sensor", content);

                // Read the response content
                var result = await response.Content.ReadAsStringAsync();

                // Check if the request was successful and return appropriate response
                if (response.IsSuccessStatusCode)
                {
                    return Ok("Sensor data sent successfully.");
                }
                else
                {
                    return StatusCode((int)response.StatusCode, result); // Return the status code with error message
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions during the request
                return BadRequest($"Error sending sensor data: {ex.Message}");
            }
        }

        
        
        [HttpPost("predict")]
        public async Task<IActionResult> PostPredictionRequest([FromBody] object modelData)
        {
            var json = JsonConvert.SerializeObject(modelData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/predict", content);
            var result = await response.Content.ReadAsStringAsync();

            return StatusCode((int)response.StatusCode, result);
        }
    }
}