using ApiGateway.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Yarp.Controllers.Iot
{
    [ApiController]
    [Route("api/iot")]
    public class IotController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public IotController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("IotAPI");

        }

        [HttpGet("experiments")]
        public async Task<IActionResult> GetAllExperiments()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<PlantExperimentDTO>>("/api/experiments");
                return Ok(response);
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Error fetching experiments from IoT backend: {ex.Message}");
            }
        }

        [HttpGet("experiments/{experimentId}")]
        public async Task<IActionResult> GetExperimentById(long experimentId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<PlantExperimentDTO>($"/api/experiments/{experimentId}");
                if (response == null)
                {
                    return NotFound();
                }
                return Ok(response);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching experiment from IoT backend: {ex.Message}");
            }
        }

        [HttpGet("experiments/{experimentId}/measurements")]
        public async Task<IActionResult> GetExperimentMeasurements(
            long experimentId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                string url = $"/api/experiments/{experimentId}/measurements";

                if (startDate.HasValue && endDate.HasValue)
                {
                    string formattedStartDate = startDate.Value.ToString("o");
                    string formattedEndDate = endDate.Value.ToString("o");
                    url += $"?startDate={Uri.EscapeDataString(formattedStartDate)}&endDate={Uri.EscapeDataString(formattedEndDate)}";
                }

                var response = await _httpClient.GetFromJsonAsync<List<PlantMeasurementsDTO>>(url);
                return Ok(response);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching measurements from IoT backend: {ex.Message}");
            }
        }

        [HttpGet("experiments/{experimentId}/measurements/latest")]
        public async Task<IActionResult> GetLatestMeasurements(long experimentId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<PlantMeasurementsDTO>>($"/api/experiments/{experimentId}/measurements/latest");
                return Ok(response);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching latest measurements from IoT backend: {ex.Message}");
            }
        }

        [HttpGet("experiments/{experimentId}/export/csv")]
        public async Task<IActionResult> ExportToCsv(
            long experimentId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                string url = $"/api/experiments/{experimentId}/export/csv";

                if (startDate.HasValue && endDate.HasValue)
                {
                    string formattedStartDate = startDate.Value.ToString("o");
                    string formattedEndDate = endDate.Value.ToString("o");
                    url += $"?startDate={Uri.EscapeDataString(formattedStartDate)}&endDate={Uri.EscapeDataString(formattedEndDate)}";
                }

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return File(
                    System.Text.Encoding.UTF8.GetBytes(content),
                    "text/csv",
                    $"experiment_{experimentId}_data.csv");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error exporting CSV data from IoT backend: {ex.Message}");
            }
        }

        [HttpGet("experiments/{experimentId}/export/json")]
        public async Task<IActionResult> ExportToJson(
            long experimentId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                string url = $"/api/experiments/{experimentId}/export/json";

                if (startDate.HasValue && endDate.HasValue)
                {
                    string formattedStartDate = startDate.Value.ToString("o");
                    string formattedEndDate = endDate.Value.ToString("o");
                    url += $"?startDate={Uri.EscapeDataString(formattedStartDate)}&endDate={Uri.EscapeDataString(formattedEndDate)}";
                }

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return File(
                    System.Text.Encoding.UTF8.GetBytes(content),
                    "application/json",
                    $"experiment_{experimentId}_data.json");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error exporting JSON data from IoT backend: {ex.Message}");
            }
        }

        [HttpPost("experiments")]
        public async Task<IActionResult> CreateExperiment([FromBody] PlantExperimentDTO experiment)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/experiments", experiment);
                response.EnsureSuccessStatusCode();

                var createdExperiment = await response.Content.ReadFromJsonAsync<PlantExperimentDTO>();
                return CreatedAtAction(nameof(GetExperimentById), new { experimentId = createdExperiment.Id }, createdExperiment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating experiment in IoT backend: {ex.Message}");
            }
        }

        [HttpPut("experiments/{experimentId}/activate")]
        public async Task<IActionResult> ActivateExperiment(long experimentId)
        {
            try
            {
                var response = await _httpClient.PutAsync($"/api/experiments/{experimentId}/activate", null);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    var errorContent = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
                    return NotFound(errorContent);
                }

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
                return Ok(content);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error activating experiment in IoT backend: {ex.Message}");
            }
        }

        [HttpGet("experiments/active")]
        public async Task<IActionResult> GetActiveExperiment()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/experiments/active");

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    var errorContent = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
                    return NotFound(errorContent);
                }

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
                return Ok(content);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching active experiment from IoT backend: {ex.Message}");
            }
        }

        [HttpDelete("experiments/{id}")]
        public async Task<IActionResult> DeleteExperiment(long id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/experiments/{id}");

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound();
                }

                response.EnsureSuccessStatusCode();
                return NoContent();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting experiment in backend service: {ex.Message}");
            }
        }
    }
}