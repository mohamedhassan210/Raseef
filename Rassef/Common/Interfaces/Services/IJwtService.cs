namespace Rassef.Common.Interfaces.Services
{
    public interface IJwtService
    {
        string GenerateToken(Guid UserId, Email Email);
        string RefreshToken();
    }
}
