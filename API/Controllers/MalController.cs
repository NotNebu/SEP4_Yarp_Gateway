using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using APII;
using Newtonsoft.Json;
using ApiGateway.DTOs;
using MLService.Models.Prediction;
using Newtonsoft.Json.Linq;

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
            var response = await _httpClient.GetAsync("/api/Sensor");
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


[HttpGet("train-model")]
public async Task<IActionResult> TrainModel()
{
    var response = await _httpClient.GetAsync("/api/sensor/train-model");
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
        public async Task<IActionResult> PredictUnified()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();

                if (string.IsNullOrWhiteSpace(body))
                    return BadRequest("Request body is empty.");

                JObject jsonObj;
                try
                {
                    jsonObj = JObject.Parse(body);
                }
                catch (JsonReaderException jex)
                {
                    return BadRequest($"Invalid JSON format: {jex.Message}");
                }

                var typeOfModel = jsonObj["TypeofModel"]?.ToString();
                if (string.IsNullOrWhiteSpace(typeOfModel))
                    return BadRequest("Missing 'TypeofModel' in request body.");

                string targetUrl;
                StringContent content;

                if (typeOfModel.Equals("logistic", StringComparison.OrdinalIgnoreCase))
                {
                    var logisticRequest = jsonObj.ToObject<LogisticPredictionRequest>();
                    var json = JsonConvert.SerializeObject(logisticRequest);
                    content = new StringContent(json, Encoding.UTF8, "application/json");
                    targetUrl = "/api/sensor/predict";
                }
                else if (typeOfModel.Equals("rfc", StringComparison.OrdinalIgnoreCase))
                {
                    var rfcRequest = jsonObj.ToObject<Rfc_PredictionRequest>();
                    var json = JsonConvert.SerializeObject(rfcRequest);
                    content = new StringContent(json, Encoding.UTF8, "application/json");
                    targetUrl = "/api/sensor/predict";
                }
                else
                {
                    return BadRequest("Unsupported TypeofModel. Use 'logistic' or 'rfc'.");
                }

                var response = await _httpClient.PostAsync(targetUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                return StatusCode((int)response.StatusCode, responseContent);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error during prediction: {ex.Message}");
            }
        }
    }
        
    
    }
    
    