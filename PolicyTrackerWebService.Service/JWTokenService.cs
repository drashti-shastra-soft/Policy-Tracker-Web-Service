using PolicyTrackerWebService.Infrastructure.IService;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PolicyTrackerWebService.Service
{
    public class JWTokenService : IJWTokenService
    {
        public string GenerateJwtToken(Domain.User mUser)
        {
            var key = Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings.Get("SecretKey"));

            int hours;
            if (!int.TryParse(ConfigurationManager.AppSettings.Get("TokenExpiryHours"), out hours) || hours <= 0)
                hours = 8;

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, string.Format("{0} {1}", mUser.FirstName, mUser.LastName).Trim()),
                    new Claim("role", mUser.Role ?? string.Empty),
                    new Claim("userId", mUser.Id.ToString()),
                    new Claim("emailAddress", mUser.EmailAddress ?? string.Empty),
                }),
                Expires = DateTime.UtcNow.AddHours(hours),
                Issuer = ConfigurationManager.AppSettings.Get("Issuer"),
                Audience = ConfigurationManager.AppSettings.Get("Audience"),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
