namespace ApiGateway.DTOs
{
    /// <summary>
    /// Data Transfer Object (DTO) til login-anmodninger via API Gateway.
    /// Indeholder brugerens email og adgangskode.
    /// </summary>
    public class LoginRequestDto
    {
        /// <summary>
        /// Brugerens emailadresse.
        /// </summary>
        public string Email { get; set; } = default!;

        /// <summary>
        /// Brugerens adgangskode i klartekst (skal sendes over HTTPS).
        /// </summary>
        public string Password { get; set; } = default!;
    }
}
