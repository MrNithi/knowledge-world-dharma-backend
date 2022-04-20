using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using knowledge_world_dharma_backend.Data;
using knowledge_world_dharma_backend.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace knowledge_world_dharma_backend.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private IConfiguration _config;
        private ApplicationDbContext _context;

        public AuthController(IConfiguration config, ApplicationDbContext context)
        {
            _context = context;
            _config = config;
        }

        // POST /authenticate/login
        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login([FromBody] UserLogin userLogin)
        {
            var user = Authenticate(userLogin);

            if (user != null)
            {
                var token = Generate(user);
                return Ok(token);
            }

            return NotFound("User not found");
        }

        private string Generate(UserModel user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Username),
                new Claim(ClaimTypes.Email, user.EmailAddress),
                new Claim(ClaimTypes.GivenName, user.GivenName),
                new Claim(ClaimTypes.Surname, user.Surname),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Audience"],
              claims,
              expires: DateTime.Now.AddMinutes(Convert.ToDouble(_config["Jwt:ExpireMin"])), // Expire in
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private UserModel Authenticate(UserLogin userLogin)
        {
            var candidateUser = _context.UserModel.FirstOrDefault(
                acc => acc.Username == userLogin.Username && acc.Password == userLogin.Password);

            if (candidateUser != null)
            {
                return candidateUser;
            }

            return null;
        }

        
        // POST: authenticate/register

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] UserModel userModel)
        {
            var candidateUser = _context.UserModel.FirstOrDefault(
                acc => acc.Username == userModel.Username || acc.EmailAddress == userModel.EmailAddress);

            if (candidateUser != null)
            {
                return BadRequest("Duplicate Username or Email!");
            }
            _context.UserModel.Add(userModel);
            _ = await _context.SaveChangesAsync();

            return CreatedAtAction("GetUserModel", new { id = userModel.Id }, userModel);
        }
    }
}
