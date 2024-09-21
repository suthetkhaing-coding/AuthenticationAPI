namespace AYASOMPO.MotorAutomation.Interfaces;

public interface IAuthService
{
    string GenerateJwtToken(string username);
}
