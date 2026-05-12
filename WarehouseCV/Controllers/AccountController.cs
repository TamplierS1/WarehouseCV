using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WarehouseCV.Models;

namespace WarehouseCV.Controllers;

public class RegisterDTO
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class LoginDTO
{
    public string Email { get; set; }
    public string Password { get; set; }
}

[ApiController]
[Route("api/[controller]s")]
public class AccountController : ControllerBase
{
    private readonly UserManager<User> _users;
    private readonly IConfiguration _config;

    public AccountController(UserManager<User> users, IConfiguration config)
    {
        _users = users;
        _config = config;
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterDTO registerInfo)
    {
        var user = new User { UserName = registerInfo.Email, Email = registerInfo.Email };
        var result = await _users.CreateAsync(user, registerInfo.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        var token = await GenerateTokenAsync(user);
        return Ok(new { token });
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginDTO loginInfo)
    {
        Console.WriteLine($"Login attempt for: {loginInfo.Email}");
        var user = await _users.FindByEmailAsync(loginInfo.Email);
        if (user is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: $"User with email {loginInfo.Email} was not found"
            );
        }

        if (!await _users.CheckPasswordAsync(user, loginInfo.Password))
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                detail: $"Incorrect password"
            );
        }

        string token = await GenerateTokenAsync(user);

        return Ok(new { token });
    }

    private async Task<string> GenerateTokenAsync(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName),
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
