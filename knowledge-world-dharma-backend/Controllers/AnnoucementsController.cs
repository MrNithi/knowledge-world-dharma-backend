using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using knowledge_world_dharma_backend.Data;
using knowledge_world_dharma_backend.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace knowledge_world_dharma_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnnoucementsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AnnoucementsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Annoucements
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAnnoucement()
        {
            var Annoucements = await _context.Annoucement.ToListAsync();
            List<object> Response = new List<object>();
            foreach (Annoucement Annoucement in Annoucements)
            {
                var _Post = await _context.Post.FindAsync(Annoucement.Post);
                var Admin = await _context.UserModel.FindAsync(Annoucement.Admin);

                Response.Add(new
                {
                    Annoucement.Id,
                    PostId = Annoucement.Post,
                    Admin = Admin.Username,
                    Post = _Post
                });
            }
            return Response;
        }

        // POST: api/annoucements
        [HttpPost("{PostId}")]
        [Authorize]
        public async Task<ActionResult<Annoucement>> PostAnnoucement(int PostId)
        {
            var CurrentUser = GetCurrentUser();

            if (CurrentUser.Role != "Admin")
            {
                return BadRequest("You're not Admin!");
            }
            var annoucement = new Annoucement
            {
                 Post = PostId,
                 Admin = CurrentUser.Id,
            };
            _context.Annoucement.Add(annoucement);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAnnoucement", new { id = annoucement.Id }, annoucement);
        }

        // DELETE: api/Annoucements/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Annoucement>> DeleteAnnoucement(int id)
        {
            var CurrentUser = GetCurrentUser();
            if (CurrentUser.Role != "Admin")
            {
                return BadRequest("You're not Admin!");
            }
            var annoucement = await _context.Annoucement.FindAsync(id);
            if (annoucement == null)
            {
                return NotFound();
            }

            _context.Annoucement.Remove(annoucement);
            await _context.SaveChangesAsync();

            return annoucement;
        }

        private UserModel GetCurrentUser()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (identity != null)
            {
                var userClaims = identity.Claims;
                var Username = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.NameIdentifier)?.Value;
                var Id = _context.UserModel.FirstOrDefault(u => u.Username == Username).Id;

                return new UserModel
                {
                    Username = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.NameIdentifier)?.Value,
                    EmailAddress = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Email)?.Value,
                    GivenName = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.GivenName)?.Value,
                    Surname = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Surname)?.Value,
                    Role = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Role)?.Value,
                    Id = Id
                };
            }
            return null;
        }
    }
}
