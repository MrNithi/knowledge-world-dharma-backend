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
    public class AnnoucementsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AnnoucementsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Annoucements
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Annoucement>>> GetAnnoucement()
        {
            return await _context.Annoucement.ToListAsync();
        }

        // GET: api/Annoucements/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Annoucement>> GetAnnoucement(int id)
        {
            var annoucement = await _context.Annoucement.FindAsync(id);

            if (annoucement == null)
            {
                return NotFound();
            }

            return annoucement;
        }

        // PUT: api/Annoucements/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAnnoucement(int id, Annoucement annoucement)
        {
            if (id != annoucement.Id)
            {
                return BadRequest();
            }

            _context.Entry(annoucement).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AnnoucementExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Annoucements
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Annoucement>> PostAnnoucement(Annoucement annoucement)
        {
            _context.Annoucement.Add(annoucement);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAnnoucement", new { id = annoucement.Id }, annoucement);
        }

        // DELETE: api/Annoucements/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Annoucement>> DeleteAnnoucement(int id)
        {
            var annoucement = await _context.Annoucement.FindAsync(id);
            if (annoucement == null)
            {
                return NotFound();
            }

            _context.Annoucement.Remove(annoucement);
            await _context.SaveChangesAsync();

            return annoucement;
        }

        private bool AnnoucementExists(int id)
        {
            return _context.Annoucement.Any(e => e.Id == id);
        }
    }
}
