using ApiGateway.DTOs;

namespace ApiGateway.Application.Interfaces
{
    public interface IUserService
    {
        Task<string> LoginAsync(string email, string password);
        Task<bool> RegisterAsync(string email, string password, string username);
        Task<UserDto?> GetUserAsync(string token);
    }
}
