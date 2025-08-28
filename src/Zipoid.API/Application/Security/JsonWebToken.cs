using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Zipoid.API.Domain;
using Zipoid.API.Models;

namespace Zipoid.API.Application.Security
{
    public class JsonWebToken : IJsonWebToken
    {
        private readonly IUserRepository _userRepository;
        private readonly JWT _jwtSettings;

        public JsonWebToken(IUserRepository userRepository, JWT jwtSettings)
        {
            _userRepository = userRepository;
            _jwtSettings = jwtSettings;
        }

        public async Task<string> GenerateTokenAsync(string email)
        {
            var client = await _userRepository.GetUserByEmailAsync(email);
            if (client == null) throw new InvalidOperationException("User not found.");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, client.Id.ToString()),
                new Claim(ClaimTypes.Email, client.Email),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes);

            var token = new JwtSecurityToken
            (
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}