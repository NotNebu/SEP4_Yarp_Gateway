using Microsoft.AspNetCore.Mvc;
using ApiGateway.Application.Interfaces;
using ApiGateway.DTOs;

namespace ApiGateway.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                var token = await _userService.LoginAsync(request.Email, request.Password);
                return Ok(new { Token = token });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Ugyldige loginoplysninger.");
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            var success = await _userService.RegisterAsync(request.Email, request.Password, request.Username);
            if (!success) return Conflict("Bruger med denne email findes allerede.");
            return Ok(new { Success = true });
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetUser()
        {
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");

            if (string.IsNullOrWhiteSpace(token))
                return Unauthorized("Token mangler.");

            var user = await _userService.GetUserAsync(token);
            if (user == null)
                return Unauthorized("Ugyldigt token.");

            return Ok(user);
        }
    }
}
