namespace Rassef.Common.Interfaces.Services.AuthenticationServices
{
    public interface IJwtService
    {
        string GenerateToken(Guid UserId, Email Email);
        string RefreshToken();
    }
}
