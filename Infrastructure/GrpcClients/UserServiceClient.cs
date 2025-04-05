using ApiGateway.Application.Interfaces;
using ApiGateway.DTOs;
using Grpc.Net.Client;
using Grpc.Core;
using UserService.Grpc;

namespace ApiGateway.Infrastructure.GrpcClients

{
    public class UserServiceClient : IUserService
    {
        private readonly AuthService.AuthServiceClient _grpcClient;

        public UserServiceClient(AuthService.AuthServiceClient grpcClient)
        {
            _grpcClient = grpcClient;
        }

        public async Task<string> LoginAsync(string email, string password)
        {
            var request = new LoginRequest { Email = email, Password = password };
            var response = await _grpcClient.LoginAsync(request);
            return response.Token;
        }

        public async Task<bool> RegisterAsync(string email, string password, string username)
        {
            var request = new RegisterRequest { Email = email, Password = password, Username = username };
            var response = await _grpcClient.RegisterAsync(request);
            return response.Success;
        }

        public async Task<UserDto?> GetUserAsync(string token)
        {
            try
            {
                var request = new UserRequest { Token = token };
                var response = await _grpcClient.GetUserAsync(request);
                return new UserDto
                {
                    Email = response.Email,
                    Username = response.Username
                };
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Unauthenticated)
            {
                return null;
            }
        }
    }
}
