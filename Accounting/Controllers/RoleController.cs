using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Accounting.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }
        // GET: api/<RoleController>
        [HttpGet]
        public async Task<ActionResult> Get()
        {
            if (_roleManager.Roles==null)
            {
                return StatusCode(404, new { Status = "Error", Message = "There is no any Role!" });
            };

            List<IdentityRole> roles = _roleManager.Roles.ToList();

            return Ok(new { Status = "Success", Roles = roles});
        }

        // GET api/<RoleController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<RoleController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] string model)
        {
            
            if (model==null)
            {
                return StatusCode(404, new { Status = "Error", Message = "Please enter valid role name!" });
            }

            IdentityRole identityRole = new();
            identityRole.Name = model;

            IdentityResult result = await _roleManager.CreateAsync(identityRole);

            if (!result.Succeeded)
            {
                return StatusCode(404, new {Status = "Error", Message = "Something went wrong" });
            }

            return Ok(new { Status = "Success", Role = identityRole });
        }

        // PUT api/<RoleController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<RoleController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
