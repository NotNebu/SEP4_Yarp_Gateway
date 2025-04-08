namespace ApiGateway.DTOs
{
    /// <summary>
    /// Data Transfer Object (DTO) til registreringsanmodninger via API Gateway.
    /// Indeholder de nødvendige oplysninger for at oprette en ny bruger.
    /// </summary>
    public class RegisterRequestDto
    {
        /// <summary>
        /// Brugerens emailadresse.
        /// </summary>
        public string Email { get; set; } = default!;

        /// <summary>
        /// Brugerens adgangskode i klartekst (skal sendes over HTTPS).
        /// </summary>
        public string Password { get; set; } = default!;

        /// <summary>
        /// Brugerens ønskede brugernavn.
        /// </summary>
        public string Username { get; set; } = default!;
    }
}
