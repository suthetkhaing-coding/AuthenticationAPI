namespace AuthenticationAPI.Interfaces;

public interface IAuthService
{
    string GenerateJwtToken(string username);
}
