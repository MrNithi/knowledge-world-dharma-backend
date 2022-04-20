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
    public class CommentApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CommentApiController(ApplicationDbContext context)
        {
            _context = context;
        }
        // GET: api/PostApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Post>>> GetPost()
        {
            var blogs = from b in _context.Post
                        where (b.Ref > 0)
                        select b;
            var data = await blogs.ToListAsync();
            return data;
        }

        // GET: api/PostApi/5
        [HttpGet("{Ref}")]
        public async Task<ActionResult<IEnumerable<Post>>> GetPost(int Ref)
        {
            var blogs = from b in _context.Post
                        where (b.Ref > 0 && b.Ref == Ref)
                        select b;
            var RefComment = await blogs.ToListAsync();
            return RefComment;
        }

        // PUT: api/PostApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPost(int id, Post post)
        {
            var currentUser = GetCurrentUser();
            var _post = await _context.Post.FindAsync(id);
            if (currentUser.Id != _post.UserId)
            {
                return BadRequest("Not owner of the post!");
            }
            if (id != post.Id || post.Ref == 0)
            {
                Console.WriteLine("error put");
                return BadRequest();
            }
            _context.Entry(post).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PostExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok();
        }

        // POST: api/PostApi
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Post>> PostPost(Post post)
        {
            var currentUser = GetCurrentUser();
            post.UserId = currentUser.Id;
            _context.Post.Add(post);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPost", new { id = post.Id }, post);
        }

        // DELETE: api/PostApi/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<Post>> DeletePost(int id)
        {
            var post = await _context.Post.FindAsync(id);
            var currentUser = GetCurrentUser();
            if (post == null || currentUser.Id != post.UserId)
            {
                return NotFound();
            }

            _context.Post.Remove(post);
            await _context.SaveChangesAsync();

            return post;
        }

        private bool PostExists(int id)
        {
            return _context.Post.Any(e => e.Id == id);
        }

        private UserModel GetCurrentUser()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (identity != null)
            {
                var userClaims = identity.Claims;

                return new UserModel
                {
                    Username = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.NameIdentifier)?.Value,
                    EmailAddress = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Email)?.Value,
                    GivenName = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.GivenName)?.Value,
                    Surname = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Surname)?.Value,
                    Role = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Role)?.Value
                };
            }
            return null;
        }
    }
}
