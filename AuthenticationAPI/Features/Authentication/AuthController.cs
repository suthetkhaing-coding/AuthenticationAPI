using AuthenticationAPI.Interfaces;
using AuthenticationAPI.Models;

namespace AuthenticationAPI.Features.Authentication;

[Route("api/[action]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly IAuthService _authService;

    public AuthController(IConfiguration config, IAuthService authService)
    {
        _config = config;
        _authService = authService;
    }

    [HttpPost]
    [AllowAnonymous]
    public IActionResult GetToken([FromBody] LoginModel model)
    {
        if (model == null)
        {
            Log.Error("Invalid request body");
            return BadRequest("Invalid request body.");
        }

        var userName = _config["Jwt:UserName"];
        var password = _config["Jwt:Password"];

        if (model.UserName == userName && model.Password == password)
        {
            var token = _authService.GenerateJwtToken(userName);
            Log.Information("Authentication is successful.!!!");
            return Ok(new { Token = token });
        }
        else
        {
            var errorResponse = new
            {
                message = "Authentication fails.!!!",
            };

            Log.Error("Authentication fails.!!!");
            return Unauthorized(errorResponse);
        }
    }
}
