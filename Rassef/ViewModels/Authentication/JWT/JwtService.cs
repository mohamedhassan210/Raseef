namespace Rassef.ViewModels.Authentication.JWT
{
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;

        public JwtService(IOptions<JwtSettings> options)
        => _jwtSettings = options.Value;

        public string GenerateToken(int UserId, Email Email)
        {
            var cliams = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,UserId.ToString()),
                new Claim(ClaimTypes.Email,Email.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);


            var tocken = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: cliams,
                signingCredentials: credentials,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes)
                );

            return new JwtSecurityTokenHandler().WriteToken(tocken);
        }

        public string RefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}
