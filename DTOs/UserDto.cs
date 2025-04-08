namespace ApiGateway.DTOs
{
    /// <summary>
    /// Data Transfer Object (DTO), der repræsenterer en bruger returneret fra autentificeringsservicen.
    /// Indeholder ikke følsomme oplysninger som adgangskode.
    /// </summary>
    public class UserDto
    {
        /// <summary>
        /// Brugerens emailadresse.
        /// </summary>
        public string Email { get; set; } = default!;

        /// <summary>
        /// Brugerens brugernavn.
        /// </summary>
        public string Username { get; set; } = default!;
    }
}
