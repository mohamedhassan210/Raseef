namespace Rassef.Common.Interfaces.Services.AuthenticationServices
{
    public interface IJwtService
    {
        string GenerateToken(int UserId, Email Email, string? name = null);
        string RefreshToken();
    }
}
