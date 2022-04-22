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
                var Owner = await _context.UserModel.FindAsync(EachPost.UserId);

                if (Owner != null)
                {
                    Res.Add((new
                    {
                        Post = EachPost,
                        Owner = Owner.Username,
                        Comments,
                        PostLikes
                    }));
                } else
                {
                    Res.Add((new
                    {
                        Post = EachPost,
                        Comments,
                        PostLikes
                    }));
                }

                
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
            var Owner = await _context.UserModel.FindAsync(Post.UserId);

            if (Post == null) 
            {
                return NotFound();
            }

            return Ok(new
            {
                Post,
                Comments,
                Owner = Owner.Username,
                PostLikes
            });
        }

        // POST: api/post
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Post>> CreatePost(Post post)
        {
            var currentUser = GetCurrentUser();
            post.UserId = currentUser.Id;
            _context.Post.Add(post);
            await _context.SaveChangesAsync();

            return CreatedAtAction("CreatePost", post);
        }

        // PUT: api/post/:id
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutPost(int id, PostForm post)
        {
            var currentUser = GetCurrentUser();
            var ExistedPost = await _context.Post.FindAsync(id);
            if (currentUser.Id != ExistedPost.UserId)
            {
                return BadRequest("Not your post!");
            }

            if (ExistedPost != null)
            {
                if (post.Title != null)
                {
                    ExistedPost.Title = post.Title;
                }
                if (post.Content != null)
                {
                    ExistedPost.Content = post.Content;
                }
                if (post.HashTag != null)
                {
                    ExistedPost.HashTag = post.HashTag;
                }
                if (post.HideStatus != 0)
                {
                    if (post.HideStatus == -1)
                    {
                        ExistedPost.HideStatus = false;
                    } else
                    {
                        ExistedPost.HideStatus = true;
                    }
                } 
            }

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

            return Ok(ExistedPost);
        }

        // DELETE: api/post/:id
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _context.Post.FindAsync(id);

            if (post == null)
            {
                return NotFound();
            }

            var currentUser = GetCurrentUser();
            if (post.UserId == currentUser.Id)
            {
                _context.Post.Remove(post);
                var QueryComments = from b in _context.Post
                              where (b.Ref == id)
                              select b;
                var Comments = await QueryComments.ToListAsync();
                var QueryLikes = from b in _context.Like
                           where (b.PostId == id)
                           select b;
                var Likes = await QueryLikes.ToListAsync();

                foreach (Post EachComment in Comments)
                {
                    _context.Post.Remove(EachComment);
                }
                foreach (Like EachLike in Likes)
                {
                    _context.Like.Remove(EachLike);
                }
                await _context.SaveChangesAsync();

                return Ok(post);
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
