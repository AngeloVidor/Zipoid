using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Zipoid.API.Application.Security;
using Zipoid.API.Domain;

namespace Zipoid.API.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJsonWebToken _tokenService;

        public AuthController(IUserService userService, IJsonWebToken tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Domain.Entities.User user)
        {
            try
            {
                var result = await _userService.AddAsync(user);
                if (result)
                    return Ok(new { message = "User registered successfully" });
                else
                    return BadRequest(new { message = "User registration failed" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(string email, string password)
        {

            try
            {
                var isValid = await _userService.LoginAsync(email, password);
                if (!isValid)
                {
                    return Unauthorized("Invalid email or password.");
                }
                var token = await _tokenService.GenerateTokenAsync(email);
                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
}