using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Workflow.Domain.Entities;

namespace Workflow.Api.Services;

public interface IJwtService
{
    string GenerateToken(AppUser user, IList<string> roles);
}

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
   _configuration = configuration;
    }

    public string GenerateToken(AppUser user, IList<string> roles)
    {
   var jwtSettings = _configuration.GetSection("Jwt");
   var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

     var claims = new List<Claim>
        {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
     new Claim(JwtRegisteredClaimNames.Email, user.Email!),
    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
     new Claim(ClaimTypes.NameIdentifier, user.Id),
       new Claim(ClaimTypes.Name, user.UserName!),
        };

        if (!string.IsNullOrEmpty(user.FullName))
        {
    claims.Add(new Claim(ClaimTypes.GivenName, user.FullName));
      }

        // Add roles as claims
        foreach (var role in roles)
        {
    claims.Add(new Claim(ClaimTypes.Role, role));
     }

  var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiryInMinutes"])),
            signingCredentials: credentials
     );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
