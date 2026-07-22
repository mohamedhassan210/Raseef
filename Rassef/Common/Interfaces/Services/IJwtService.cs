namespace Rassef.Common.Interfaces.Services
{
    public interface IJwtService
    {
        string GenerateToken();
        string RefreshToken();
    }
}
