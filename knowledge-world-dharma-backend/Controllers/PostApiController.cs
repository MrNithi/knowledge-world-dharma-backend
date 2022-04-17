using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using knowledge_world_dharma_backend.Data;
using knowledge_world_dharma_backend.Models;
using Newtonsoft.Json;
using System.Collections;

namespace knowledge_world_dharma_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PostApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/PostApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Post>>> GetPost()
        {
            var blogs = from b in _context.Post
                        where (b.Ref == 0)
                        select b;
            var Posts = await blogs.ToListAsync();
            List<Object> Res = new List<Object>();
            foreach (Post aPost in Posts)
            {
                var alike = from b in _context.Like
                            where (b.PostId == aPost.Id)
                            select b;
                var likes = await alike.ToListAsync();
                var aComment = from b in _context.Post
                            where (b.Ref == aPost.Id)
                            select b;
                var Comments = await aComment.ToListAsync();

                var CommentPost = new { comment = Comments };
                var LikePost = new { Like = likes };
                var Post = new { post = aPost };
                ArrayList Mapdata = new ArrayList();
                Mapdata.Add(Post);
                Mapdata.Add(CommentPost);
                Mapdata.Add(LikePost);
                Res.Add((new { PostItem = Mapdata }));
            }
            return Ok(Res); ;
        }
        // GET: api/PostApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<JsonResult>>> GetPostid(int id)
        {
            var post = await _context.Post.FindAsync(id);
            var Comment = from b in _context.Post
                       where (b.Ref == id && b.HideStatus == false)
                       select b;
            var Comments = await Comment.ToListAsync();
            var like = from b in _context.Like
                          where (b.PostId == id)
                          select b;
            var Likes = await like.ToListAsync();
            if (post == null || Comments == null) 
            {
                return NotFound();
            }
            return Ok(new { post = post, comment = Comments ,like = Likes });
        }

        // PUT: api/PostApi/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPost(int id, Post post)
        {
            if (id != post.Id || post.Ref != 0)
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

        // POST: api/PostApi
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Post>> PostPost(Post post)
        {
            _context.Post.Add(post);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPost", new { id = post.Id }, post);
        }

        // DELETE: api/PostApi/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Post>> DeletePost(int id)
        {
            var post = await _context.Post.FindAsync(id);
            var Comment = from b in _context.Post
                           where (b.Ref == id )
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
        private bool PostExists(int id)
        {
            return _context.Post.Any(e => e.Id == id);
        }
    }
}
