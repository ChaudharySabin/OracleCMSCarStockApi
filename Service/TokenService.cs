using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using api.Interfaces;
using api.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;

namespace api.Service
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly UserManager<User> _userManager;
        public TokenService(IConfiguration config, UserManager<User> userManager)
        {
            _config = config;
            _userManager = userManager;
        }

        public async Task<string> CreateToken(User user)
        {

            var roles = await _userManager.GetRolesAsync(user);


            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim("dealerId", user.DealerId?.ToString() ?? string.Empty)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }


            var jwtSettings = _config.GetSection("JWT");
            if (string.IsNullOrWhiteSpace(jwtSettings["SigningKey"]))
            {
                throw new KeyNotFoundException("Signing Key is not set");
            }
            var keyInBytes = Encoding.UTF8.GetBytes(jwtSettings["SigningKey"]!);
            var creds = new SigningCredentials(
                new SymmetricSecurityKey(keyInBytes),
                SecurityAlgorithms.HmacSha512
            );


            var expiryString = jwtSettings["ExpiryMinutes"];
            if (!double.TryParse(expiryString, CultureInfo.InvariantCulture, out double expiryMinutes))
            {
                //
                expiryMinutes = 60; //Fallback to 60 if there is any error in parsing;
            }
            var expiryDateOfToken = DateTime.Now.AddMinutes(expiryMinutes);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: expiryDateOfToken,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);

        }
    }
}