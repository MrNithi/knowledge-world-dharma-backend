﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using knowledge_world_dharma_backend.Data;
using knowledge_world_dharma_backend.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace knowledge_world_dharma_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LikeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LikeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/like
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Like>>> GetLikes()
        {
            return await _context.Like.ToListAsync();
        }

        // GET: api/like/:id
        // get like of post with id
        [HttpGet("{id}")]
        [AllowAnonymous]
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

        // POST: api/like
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Like>> LikeLike(Like like)
        {
            var currentUser = GetCurrentUser();
            if (currentUser.Banned)
            {
                return BadRequest("You're banned!");
            }
            var query = await _context.Like
               .FirstOrDefaultAsync(item => item.PostId == like.PostId && item.UserId == currentUser.Id);
            if (query != null)
            {
                return BadRequest();
            }
            var NewLike = new Like
            {
                PostId = like.PostId,
                Emoji = like.Emoji,
                UserId = currentUser.Id,
            };
            _context.Like.Add(NewLike);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetPost", new { id = like.Id }, like);
        }

        // DELETE: api/PostApi/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<Like>> UnLike(int Id)
        {
            var currentUser = GetCurrentUser();
            if (currentUser.Banned)
            {
                return BadRequest("You're banned!");
            }
            var unLike = await _context.Like
               .FirstOrDefaultAsync(item => item.UserId == currentUser.Id && item.PostId == Id);
            if (unLike == null)
            {
                return NotFound();
            }
            _context.Like.Remove(unLike);
            await _context.SaveChangesAsync();
            return Ok();
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
    }
}