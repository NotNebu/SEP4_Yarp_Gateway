using ApiGateway.Application.Interfaces;
using ApiGateway.DTOs;
using Grpc.Core;
using Grpc.Net.Client;
using UserService.Grpc;

namespace ApiGateway.Infrastructure.GrpcClients
{
    /// <summary>
    /// gRPC-klient til kommunikation med bruger-microservicen via AuthService.
    /// Implementerer IUserService-interfacet og videresender kald via gRPC.
    /// </summary>
    public class UserServiceClient : IUserService
    {
        private readonly AuthService.AuthServiceClient _grpcClient;

        /// <summary>
        /// Initialiserer en ny instans af <see cref="UserServiceClient"/> med en gRPC-klient.
        /// </summary>
        /// <param name="grpcClient">En instans af den genererede gRPC AuthService-klient.</param>
        public UserServiceClient(AuthService.AuthServiceClient grpcClient)
        {
            _grpcClient = grpcClient;
        }

        /// <summary>
        /// Sender en login-anmodning til bruger-servicen via gRPC og returnerer et JWT-token.
        /// </summary>
        /// <param name="email">Brugerens emailadresse.</param>
        /// <param name="password">Brugerens adgangskode.</param>
        /// <returns>Et JWT-token som streng.</returns>
        public async Task<string> LoginAsync(string email, string password)
        {
            var request = new LoginRequest { Email = email, Password = password };
            var response = await _grpcClient.LoginAsync(request);
            return response.Token;
        }

        /// <summary>
        /// Sender en registreringsanmodning til bruger-servicen via gRPC.
        /// </summary>
        /// <param name="email">Brugerens emailadresse.</param>
        /// <param name="password">Adgangskode i klartekst (sendt via HTTPS).</param>
        /// <param name="username">Brugerens ønskede brugernavn.</param>
        /// <returns>True hvis registreringen lykkedes; ellers false.</returns>
        public async Task<bool> RegisterAsync(string email, string password, string username)
        {
            var request = new RegisterRequest
            {
                Email = email,
                Password = password,
                Username = username,
            };
            var response = await _grpcClient.RegisterAsync(request);
            return response.Success;
        }

        /// <summary>
        /// Henter brugerdata baseret på et JWT-token.
        /// Returnerer null hvis tokenet er ugyldigt.
        /// </summary>
        /// <param name="token">JWT-token som identificerer brugeren.</param>
        /// <returns>Et <see cref="UserDto"/> objekt hvis tokenet er gyldigt; ellers null.</returns>
        public async Task<UserDto?> GetUserAsync(string token)
        {
            try
            {
                var request = new UserRequest { Token = token };
                var response = await _grpcClient.GetUserAsync(request);
                return new UserDto { Email = response.Email, Username = response.Username };
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Unauthenticated)
            {
                return null;
            }
        }
    }
}
