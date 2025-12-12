
namespace Transit.Application.Options
{
    public class JwtSettings
    {
        public string SigningKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string[] Audiences { get; set; }
    }
}
