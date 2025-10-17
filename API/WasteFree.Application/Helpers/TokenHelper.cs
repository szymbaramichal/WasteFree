using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace WasteFree.Application.Helpers;

public static class TokenHelper
{
    public static string GenerateJwtToken(string userName, string id, int role, string key)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var keyArray = Encoding.UTF8.GetBytes(key);
        
        if (keyArray.Length < 32)
        {
            throw new ArgumentException("Key must be at least 32 bytes long.");
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(ClaimTypes.NameIdentifier, id),
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Role, role.ToString())
            ]),
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyArray), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}