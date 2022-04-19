using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using knowledge_world_dharma_backend.Data;
using knowledge_world_dharma_backend.Models;

namespace knowledge_world_dharma_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LikeApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LikeApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/PostApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Like>>> GetLike()
        {
            return await _context.Like.ToListAsync();
        }

        // GET: api/PostApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Like>> GetLike(int id)
        {
            var data = from item in _context.Like where (item.PostId == id) select item;
            var likes = await data.ToListAsync();
            if (likes == null)
            {
                return NotFound();
            }
            return Ok(likes);
        }

        // POST: api/PostApi
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Like>> LikeLike(Like like)
        {
            var query = await _context.Like
               .FirstOrDefaultAsync(item => item.PostId == like.PostId && item.UserId == like.UserId);
            if (query != null)
            {
                return BadRequest();
            }
            _context.Like.Add(like);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetPost", new { id = like.Id }, like);
        }

        // DELETE: api/PostApi/5
        [HttpDelete]
        public async Task<ActionResult<Like>> UnLike(Like like)
        {
            var unLike = await _context.Like
               .FirstOrDefaultAsync(item => item.PostId == like.PostId && item.UserId == like.UserId);
            if (unLike == null)
            {
                return NotFound();
            }
            _context.Like.Remove(unLike);
            await _context.SaveChangesAsync();
            return Ok();
        }

        private bool PostExists(int id)
        {
            return _context.Like.Any(e => e.Id == id);
        }
    }
}