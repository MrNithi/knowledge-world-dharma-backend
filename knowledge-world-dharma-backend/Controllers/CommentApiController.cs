﻿using System;
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
            //var comment = from b in _context.Post
            //           where (b.Id == Ref)
            //            select b;
            // var IdComment = await comment.ToListAsync();
            // List<List<Post>> Res = new List<List<Post>>();
            //Res.Add(IdComment);
            // Res.Add(RefComment);
            // return Ok(Res);
        }
        // PUT: api/PostApi/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPost(int id, Post post)
        {
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
            if (post == null)
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
    }
}
