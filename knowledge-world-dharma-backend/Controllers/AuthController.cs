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

        // GET /auth/profiles
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult> Profiles()
        {
            // Query for Post
            var Users = await _context.UserModel.ToListAsync();
            List<object> Res = new List<object>();

            foreach (UserModel EachUser in Users)
            {
                Res.Add(new
                {
                    EachUser.Id,
                    EachUser.Username,
                    EachUser.EmailAddress,
                    EachUser.Role,
                    EachUser.GivenName,
                    EachUser.Surname,
                    EachUser.Banned
                });
            }

            return Ok(Res);
        }
        // GET /auth/profile
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var currentUser = GetCurrentUser();
            var StoredUser = await _context.UserModel.FindAsync(currentUser.Id);

            if (currentUser != null)
            {
                return Ok(new
                {
                    currentUser.Id,
                    currentUser.Username,
                    currentUser.EmailAddress,
                    currentUser.Role,
                    StoredUser.GivenName,
                    StoredUser.Surname,
                    StoredUser.Banned
                });
            }

            return NotFound("No User with this ID!");
        }

        // POST /auth/login
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

        // POST: auth/register
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] UserModel userModel)
        {
            if (userModel.Username == null
                || userModel.Password == null
                || userModel.EmailAddress == null)
            {
                return BadRequest("Not enough body!");
            }

            var candidateUser = _context.UserModel.FirstOrDefault(
                acc => acc.Username == userModel.Username || acc.EmailAddress == userModel.EmailAddress);

            if (candidateUser != null)
            {
                return BadRequest("Duplicate Username or Email!");
            }

            var storedUser = new UserModel
            {
                EmailAddress = userModel.EmailAddress,
                Username = userModel.Username,
                Password = sha256(userModel.Password),
                Role = "User", // Default
                GivenName = userModel.GivenName,
                Surname = userModel.Surname
            };
            _context.UserModel.Add(storedUser);
            _ = await _context.SaveChangesAsync();

            var token = Generate(storedUser);

            return CreatedAtAction("Token", token);
        }

        // PUT /auth/editProfile
        [Authorize]
        [HttpPut("{UserId}")]
        public async Task<IActionResult> EditProfile(int UserId, UserForm UserData)
        {
            var CurrentUser = GetCurrentUser();
            var StoredUser = await _context.UserModel.FindAsync(CurrentUser.Id);

            if (CurrentUser.Id != UserId && CurrentUser.Role != "Admin") {
                return BadRequest("No Permission!");
            }

            if (StoredUser != null)
            {
                if (UserData.Username != null)
                {
                    StoredUser.Username = UserData.Username;
                }
                if (UserData.EmailAddress != null)
                {
                    StoredUser.EmailAddress = UserData.EmailAddress;
                }
                if (UserData.GivenName != null)
                {
                    StoredUser.GivenName = UserData.GivenName;
                }
                if (UserData.Surname != null)
                {
                    StoredUser.Surname = UserData.Surname;
                }
                if (UserData.Password != null)
                {
                    StoredUser.Password = sha256(UserData.Password);
                }

                await _context.SaveChangesAsync();
                return Ok("User data is saved!");
            }

            return NotFound();
        }

        // PUT /auth/setAdmin
        [Authorize]
        [HttpPut("{UserId}")]
        public async Task<IActionResult> SetAdmin(int UserId)
        {
            var CurrentUser = GetCurrentUser();
            var CandidateUser = await _context.UserModel.FindAsync(UserId);
            
            if (CurrentUser.Role != "Admin")
            {
                return NotFound("You have no right!");
            }
            

            if (CandidateUser != null)
            {
                
                if (CandidateUser.Role == "Admin")
                {
                    CandidateUser.Role = "User";
                } else
                {
                    CandidateUser.Role = "Admin";
                }
                await _context.SaveChangesAsync();
                return Ok("User " + CandidateUser.Username + " is setted as " + CandidateUser.Role + "!");
            }

            return NotFound("User not found");
        }

        // PUT /auth/ban
        [Authorize]
        [HttpPut("{UserId}")]
        public async Task<IActionResult> Ban(int UserId)
        {
            var CurrentUser = GetCurrentUser();
            var CandidateUser = await _context.UserModel.FindAsync(UserId);

            if (CurrentUser.Role != "Admin")
            {
                return NotFound("You have no right!");
            }

            if (CandidateUser != null)
            {
                CandidateUser.Banned = !CandidateUser.Banned;
                await _context.SaveChangesAsync();
                return Ok("User " + CandidateUser.Username + " ban status is setted as " + CandidateUser.Banned + "!");
            }

            return NotFound("User not found");
        }

        // DELETE: /auth/unregister
        [HttpDelete]
        [Authorize]
        public async Task<ActionResult<Like>> Unregister()
        {
            var currentUser = GetCurrentUser();
            var StoredUser = await _context.UserModel
               .FirstOrDefaultAsync(item => item.Id == currentUser.Id);
            if (StoredUser == null)
            {
                return NotFound();
            }
            _context.UserModel.Remove(StoredUser);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // DELETE: /auth/banish/:id
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> Banish(int Id)
        {
            var currentUser = GetCurrentUser();
            if (currentUser.Role != "Admin")
            {
                return BadRequest("You're not admin!");
            }
            var StoredUser = await _context.UserModel
               .FirstOrDefaultAsync(item => item.Id == Id);
            if (StoredUser == null)
            {
                return NotFound();
            }
            _context.UserModel.Remove(StoredUser);
            await _context.SaveChangesAsync();
            return Ok();
        }

        private string Generate(UserModel user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Username),
                new Claim(ClaimTypes.Email, user.EmailAddress),
                new Claim(ClaimTypes.Role, user.Role),
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
                acc => acc.Username == userLogin.Username
                && acc.Password == sha256(userLogin.Password)
            );

            if (candidateUser != null)
            {
                return candidateUser;
            }

            return null;
        }

        private UserModel GetCurrentUser()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (identity != null)
            {
                var userClaims = identity.Claims;
                var Username = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.NameIdentifier)?.Value;
                var User = _context.UserModel.FirstOrDefault(u => u.Username == Username);

                if (User == null)
                {
                    return null;
                }

                return new UserModel
                {
                    Username = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.NameIdentifier)?.Value,
                    EmailAddress = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Email)?.Value,
                    GivenName = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.GivenName)?.Value,
                    Surname = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Surname)?.Value,
                    Role = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Role)?.Value,
                    Id = User.Id,
                    Banned = User.Banned
                };
            }
            return null;
        }

        // Helper Hashing function
        static string sha256(string randomString)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(randomString));
            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
        }
    }
}
