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
    public class PostController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PostController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/post
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Post>>> PostList()
        {
            // Query for Post
            var PostsWithoutRef = from b in _context.Post
                        where (b.Ref == 0)
                        select b;
            var Posts = await PostsWithoutRef.ToListAsync();
            List<object> Res = new List<object>();

            foreach (Post EachPost in Posts)
            {
                var QueryLikes = from like in _context.Like
                            where (like.PostId == EachPost.Id)
                            select like;
                var PostLikes = await QueryLikes.ToListAsync();
                var QueryComments = from comment in _context.Post
                            where (comment.Ref == EachPost.Id)
                            select comment;
                var Comments = await QueryComments.ToListAsync();

                Res.Add((new {
                    Post = EachPost,
                    Comments,
                    PostLikes
                }));
            }
            return Ok(Res);
        }

        // GET: api/post/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<JsonResult>>> PostDetail(int id)
        {
            var Post = await _context.Post.FindAsync(id);
            var QueryLikes = from like in _context.Like
                             where (like.PostId == Post.Id)
                             select like;
            var PostLikes = await QueryLikes.ToListAsync();
            var QueryComments = from comment in _context.Post
                                where (comment.Ref == Post.Id)
                                select comment;
            var Comments = await QueryComments.ToListAsync();

            if (Post == null) 
            {
                return NotFound();
            }

            return Ok(new
            {
                Post,
                Comments,
                PostLikes
            });
        }

        // POST: api/PostApi
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Post>> CreatePost(Post post)
        {
            var currentUser = GetCurrentUser();
            post.UserId = currentUser.Id;
            //_context.Post.Add(post);
            //await _context.SaveChangesAsync();

            return CreatedAtAction("GetPost", currentUser);
        }

        // PUT: api/post/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutPost(int id, Post post)
        {
            var currentUser = GetCurrentUser();
            // This is weird prob need check later
            if (id != post.Id || post.Ref != 0 || currentUser.Id != post.UserId)
            {
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

        // DELETE: api/PostApi/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<Post>> DeletePost(int id)
        {
            var post = await _context.Post.FindAsync(id);
            var currentUser = GetCurrentUser();
            if (post.UserId == currentUser.Id)
            {
                var Comment = from b in _context.Post
                              where (b.Ref == id)
                              select b;
                var Comments = await Comment.ToListAsync();
                var Like = from b in _context.Like
                           where (b.PostId == id)
                           select b;
                var Likes = await Like.ToListAsync();
                if (post == null)
                {
                    return NotFound();
                }

                _context.Post.Remove(post);
                foreach (Post aPost in Comments)
                {
                    _context.Post.Remove(aPost);
                }
                foreach (Like aLike in Likes)
                {
                    _context.Like.Remove(aLike);
                }
                await _context.SaveChangesAsync();

                return post;
            }
            return BadRequest("Not your post!");
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
