using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Accounting.Data;
using Accounting.Model;
using Accounting.ViewModel;

namespace Accounting.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PositionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PositionController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Position
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Position>>> GetPositions()
        {
          if (_context.Positions == null)
          {
              return NotFound();
          }
            return await _context.Positions.ToListAsync();
        }

        // GET: api/Position/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Position>> GetPosition(int id)
        {
          if (_context.Positions == null)
          {
              return NotFound();
          }
            var position = await _context.Positions.FindAsync(id);

            if (position == null)
            {
                return NotFound();
            }

            return position;
        }

        // PUT: api/Position/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPosition(int id, Position position)
        {
            if (id != position.Id)
            {
                return BadRequest();
            }

            _context.Entry(position).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PositionExists(id))
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

        // POST: api/Position
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Position>> PostPosition(PositionModel model)
        {
            if (!ModelState.IsValid)
            {
                return Problem("Please Enter valid Value!");
            }

            if (!_context.Companies.Any(c => c.Id == model.CompanyId))
            {
                return Problem("There is no such Company Id!");
            }

            Position position = new()
            {
                CreatedAt = DateTime.Now,
                CompanyId = model.CompanyId,
                PositionName = model.PositionName,
                Salary = model.Salary, 
                UpdatedAt = null,
                Status = true,
            };

            _context.Positions.Add(position);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPosition", new { id = position.Id }, position);
        }

        // DELETE: api/Position/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePosition(int id)
        {
            if (_context.Positions == null)
            {
                return NotFound();
            }
            var position = await _context.Positions.FindAsync(id);
            if (position == null)
            {
                return NotFound();
            }

            _context.Positions.Remove(position);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("position-tree/{id}")]
        public async Task<IActionResult> GetPositionTree(int id)
        {
            List<Position> positions = await _context.Positions.ToListAsync();


            return Ok(positions.Where(c => c.CompanyId==id).Select(c => new
            {
                Id = c.Id,
                Key = c.Id,
                Name = c.PositionName+"-"+c.Salary

            }).ToList());
        }

        private bool PositionExists(int id)
        {
            return (_context.Positions?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
