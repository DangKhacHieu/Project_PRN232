using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DAL.Data;
using DAL.Entities;

namespace API_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContractHistoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ContractHistoriesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ContractHistories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ContractHistory>>> GetContractHistories()
        {
            return await _context.ContractHistories.ToListAsync();
        }

        // GET: api/ContractHistories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ContractHistory>> GetContractHistory(int id)
        {
            var contractHistory = await _context.ContractHistories.FindAsync(id);

            if (contractHistory == null)
            {
                return NotFound();
            }

            return contractHistory;
        }

        // PUT: api/ContractHistories/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutContractHistory(int id, ContractHistory contractHistory)
        {
            if (id != contractHistory.HistoryId)
            {
                return BadRequest();
            }

            _context.Entry(contractHistory).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContractHistoryExists(id))
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

        // POST: api/ContractHistories
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ContractHistory>> PostContractHistory(ContractHistory contractHistory)
        {
            _context.ContractHistories.Add(contractHistory);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetContractHistory", new { id = contractHistory.HistoryId }, contractHistory);
        }

        // DELETE: api/ContractHistories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContractHistory(int id)
        {
            var contractHistory = await _context.ContractHistories.FindAsync(id);
            if (contractHistory == null)
            {
                return NotFound();
            }

            _context.ContractHistories.Remove(contractHistory);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ContractHistoryExists(int id)
        {
            return _context.ContractHistories.Any(e => e.HistoryId == id);
        }


    }
}
