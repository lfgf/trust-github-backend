using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Kojinx.Trust.GitHub;

public class GitHubStateSigner
{
    private readonly string _jwtKey;
    private readonly string? _issuer;
    private readonly string? _audience;

    public GitHubStateSigner(string jwtKey, string? issuer = null, string? audience = null)
    {
        _jwtKey = jwtKey;
        _issuer = issuer;
        _audience = audience;
    }

    public string GenerateState(string userId)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim("type", "github_link_state"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string? ValidateState(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        
        if (!tokenHandler.CanReadToken(token)) return null;

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var typeClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "type")?.Value;
            if (typeClaim != "github_link_state") return null;

            return jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value;
        }
        catch
        {
            return null;
        }
    }
}
