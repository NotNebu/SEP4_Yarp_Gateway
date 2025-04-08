using ApiGateway.DTOs;

namespace ApiGateway.Application.Interfaces
{
    /// <summary>
    /// Interface for kommunikation med bruger-autentificeringssystemet via gatewayen.
    /// Håndterer login, registrering og hentning af brugeroplysninger baseret på JWT-token.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Logger en bruger ind med email og adgangskode.
        /// </summary>
        /// <param name="email">Brugerens emailadresse.</param>
        /// <param name="password">Brugerens adgangskode.</param>
        /// <returns>En JWT-token som streng, hvis login lykkes.</returns>
        Task<string> LoginAsync(string email, string password);

        /// <summary>
        /// Registrerer en ny bruger med email, adgangskode og brugernavn.
        /// </summary>
        /// <param name="email">Brugerens emailadresse.</param>
        /// <param name="password">Brugerens ønskede adgangskode.</param>
        /// <param name="username">Brugerens ønskede brugernavn.</param>
        /// <returns>True hvis registreringen lykkes; ellers false.</returns>
        Task<bool> RegisterAsync(string email, string password, string username);

        /// <summary>
        /// Henter brugeroplysninger baseret på JWT-token.
        /// </summary>
        /// <param name="token">JWT-token, der identificerer brugeren.</param>
        /// <returns><see cref="UserDto"/> med email og brugernavn, hvis token er gyldigt; ellers null.</returns>
        Task<UserDto?> GetUserAsync(string token);
    }
}
