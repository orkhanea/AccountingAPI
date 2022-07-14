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
    public class TaxController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TaxController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Tax
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tax>>> GetTaxes()
        {
          if (_context.Taxes == null)
          {
              return NotFound();
          }
            return await _context.Taxes.ToListAsync();
        }

        // GET: api/Tax/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Tax>> GetTax(int id)
        {
          if (_context.Taxes == null)
          {
              return NotFound();
          }
            var tax = await _context.Taxes.FindAsync(id);

            if (tax == null)
            {
                return NotFound();
            }

            return tax;
        }

        // PUT: api/Tax/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTax(int id, Tax tax)
        {
            if (id != tax.Id)
            {
                return BadRequest();
            }

            _context.Entry(tax).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaxExists(id))
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

        // POST: api/Tax
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Tax>> PostTax(TaxModel model)
        {
          if (_context.Taxes == null)
          {
              return Problem("Entity set 'AppDbContext.Taxes'  is null.");
          }

            Tax tax = new()
            {
                Name = model.Name,
                Persentage = model.Persentage,
                Type = model.Type,
                CreatedAt = DateTime.Now
            };
            _context.Taxes.Add(tax);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTax", new { id = tax.Id }, tax);
        }

        // DELETE: api/Tax/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTax(int id)
        {
            if (_context.Taxes == null)
            {
                return NotFound();
            }
            var tax = await _context.Taxes.FindAsync(id);
            if (tax == null)
            {
                return NotFound();
            }

            _context.Taxes.Remove(tax);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TaxExists(int id)
        {
            return (_context.Taxes?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
