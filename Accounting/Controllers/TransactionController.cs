using Accounting.Data;
using Accounting.Model;
using Accounting.Model.DTOs;
using Accounting.ViewModel;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Accounting.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public TransactionController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/<TransactionController>
        [HttpGet]
        public async Task<ActionResult> Get()
        {
            if (await _context.Transactions.ToListAsync() == null)
            {
                return StatusCode(404);
            }
            List <Transaction> transactions = await _context.Transactions.Include(t=>t.RecieverCompany).Include(t=>t.SenderCompany).ToListAsync();

            List<TransactionDTO> transactionDTOs = new();
            foreach (var transaction in transactions)
            {
                TransactionDTO transactionDTO = _mapper.Map<Transaction, TransactionDTO>(transaction);
                transactionDTOs.Add(transactionDTO);
            }

            return Ok(new { Status = "Success", Trasnsactions = transactionDTOs});
        }

        // GET api/<TransactionController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int? id)
        {
            if (id==null)
            {
                return StatusCode(404, new { Status = "Error", Message = "Id should be written!" });
            }

            if (await _context.Transactions.FindAsync(id)==null)
            {
                return StatusCode(404, new { Status = "Error", Message = "There is no such id" });
            }

            TransactionDTO transactionDTO = _mapper.Map<Transaction, TransactionDTO>(await _context.Transactions.Include(t=>t.SenderCompany).Include(t=>t.RecieverCompany).Where(t=>t.Id==id).FirstOrDefaultAsync());
            return Ok(new { Status = "Success", Transaction = transactionDTO });  

        }

        // POST api/<TransactionController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] TransactionModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.ValidationState);
            }

            if (!_context.Companies.Any(c => c.Id == model.SenderCompanyId) && !_context.Companies.Any(c => c.Id == model.RecieverCompanyId))
            {
                return Problem("There is no such Company Id!");
            }

            Transaction transaction = new()
            {
                CreatedDate = DateTime.Now,
                Note = model.Note,
                RecieverCompanyId = model.RecieverCompanyId,
                SenderCompanyId = model.SenderCompanyId,
                NETAmount = model.NETAmount,
                RetiredDate = model.RetiredDate,
                SendDate = model.SendDate,
                TAXAmount = model.TAXAmount

            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(new { Status = "Success", Transaction = model});

        }

        // PUT api/<TransactionController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<TransactionController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
