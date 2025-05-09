using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using APII;
using Newtonsoft.Json;
using ApiGateway.DTOs;
using MLService.Models.Prediction;

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

        
        
        [HttpPost("rfc")]
        public async Task<IActionResult> PostRfcPrediction([FromBody] Rfc_PredictionRequest data)
        {
            try
            {
                // Serialize the prediction request object into JSON
                var json = JsonConvert.SerializeObject(data);

                // Create the content with the serialized JSON
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Make the POST request to the ML service for RFC prediction
                var response = await _httpClient.PostAsync("/api/prediction/rfc-predict", content);

                // Read the response content
                var result = await response.Content.ReadAsStringAsync();

                // Return appropriate response based on success or failure
                return StatusCode((int)response.StatusCode, result);
            }
            catch (Exception ex)
            {
                // Handle any exceptions during the request
                return BadRequest($"Error during RFC prediction: {ex.Message}");
            }
        }
        [HttpPost("logistic")]
        public async Task<IActionResult> PostLogisticPrediction([FromBody] LogisticPredictionRequest data)
        {
            try
            {
                // Serialize the prediction request object into JSON
                var json = JsonConvert.SerializeObject(data);

                // Create the content with the serialized JSON
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Make the POST request to the ML service for Logistic prediction
                var response = await _httpClient.PostAsync("/api/prediction/logistic", content);

                // Read the response content
                var result = await response.Content.ReadAsStringAsync();

                // Return appropriate response based on success or failure
                return StatusCode((int)response.StatusCode, result);
            }
            catch (Exception ex)
            {
                // Handle any exceptions during the request
                return BadRequest($"Error during Logistic prediction: {ex.Message}");
            }
        }
    }
    
    //
}