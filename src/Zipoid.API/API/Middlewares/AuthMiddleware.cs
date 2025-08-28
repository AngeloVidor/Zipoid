using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Zipoid.API.Models;

namespace Zipoid.API.API.Middlewares
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly JWT _jwtSettings;

        public AuthMiddleware(RequestDelegate next, JWT jwtConfig)
        {
            _next = next;
            _jwtSettings = jwtConfig;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();
            if (path.StartsWith("/api/auth/login") ||
                path.StartsWith("/api/auth/register"))
            {
                await _next(context);
                return;
            }

            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                throw new Exception("Token not provided");
            }

            var tokenAccess = authHeader.Split(" ")[1];

            var isValid = ValidateToken(tokenAccess);
            if (!isValid) throw new Exception("Invalid token");

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(tokenAccess) as JwtSecurityToken;
            var userClaims = jwtToken?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userClaims == null) throw new Exception("UserId not found in claims");

            if (!Guid.TryParse(userClaims.Value, out var userId))
                throw new Exception("UserId in token is not a valid GUID");

            context.Items["userId"] = userId;

            await _next(context);
        }

        public bool ValidateToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var secretKey = _jwtSettings.Key;
                if (string.IsNullOrEmpty(secretKey)) throw new Exception("JWT Secret Key is not configured");

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidAudience = _jwtSettings.Audience,
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = handler.ValidateToken(token, tokenValidationParameters, out var validatedToken);

                if (!(validatedToken is JwtSecurityToken jwtSecurityToken)) throw new Exception("Invalid token");

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Token validation failed: {ex.Message}");
            }
        }
    }
}