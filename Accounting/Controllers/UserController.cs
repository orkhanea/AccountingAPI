using Accounting.Data;
using Accounting.Helper;
using Accounting.Model;
using Accounting.Model.DTOs;
using Accounting.ViewModel;
using AngouriMath;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebSuperApi.ModelStates;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Accounting.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;

        public UserController(AppDbContext context, RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration configuration)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
            _configuration = configuration;
        }

        // GET: api/<UserController>
        [HttpGet]
        [Authorize(Roles = "Admin, Human Resources")]
        public async Task<ActionResult> Get()
        {
            string role = User.FindFirst(ClaimTypes.Role).Value;
            HelperClass helperClass = new();

            if (role=="Admin")
            {
                List<User> users = _context.Users.Include(u => u.Position).ThenInclude(p => p.Company).ToList();
                List<Tax> taxes = _context.Taxes.ToList();

                return Ok(new { Status = "Success", Users = helperClass.GetUser(taxes, null, users) });
            }

            if (role == "Human Resources")
            {
                string id = User.FindFirst(ClaimTypes.Name).Value;
                User user = _context.Users.Include(u => u.Position).ThenInclude(p => p.Company).Where(u => u.Id == id).FirstOrDefault();
                List<User> users = _context.Users.Include(u => u.Position).ThenInclude(p => p.Company).Where(p=>p.Position.CompanyId==user.Position.CompanyId).ToList();
                List<Tax> taxes = _context.Taxes.ToList();

                return Ok(new { Status = "Success", Users = helperClass.GetUser(taxes, null, users) });
            }

            return StatusCode(404, new { Status = "Error", Message = "Something went wrong!" });
            
            
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Director, Human Resources")]
        [Route("get-users-by-company")]
        public async Task<ActionResult> GetUserByCompanyId()
        {
            string id = User.FindFirst(ClaimTypes.Name).Value;
            var user = await _context.Users.FindAsync(id);
            List<User> users = _context.Users.Include(u => u.Position).ThenInclude(p => p.Company).Where(u => u.Position.CompanyId == user.PositionId).ToList();
            List<Tax> taxes = _context.Taxes.ToList();
            HelperClass helperClass = new();
            return Ok(new { Status = "Success", Users = helperClass.GetUser(taxes, null, users) });
        }

        // GET api/<UserController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<UserController>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Post([FromBody] UserModel model)
        {
            if (ModelState.IsValid)
            {
                if (!_context.Positions.Any(p => p.Id == model.PositionId))
                {
                    return StatusCode(401, new { Status = "Error", Message = "You dont have permission!" });
                }

                if (!_roleManager.Roles.Any(t=>t.Id == model.RoleId))
                {
                    return StatusCode(404, new { StatusCode = "Error", Message = "There is no such role" });
                }

                User user = new()
                {
                    Name = model.Name,
                    Surname = model.Surname,
                    BDate = model.BDate,
                    EmploymentDate = model.EmploymentDate,
                    Email = model.Mail,
                    PositionId = model.PositionId,
                    CreatedAt = DateTime.Now,
                    Status = true,
                    UserName = model.Mail
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                await _context.SaveChangesAsync();
                if (result.Succeeded)
                {
                    var _user = _userManager.Users.FirstOrDefault(u => u.Id == user.Id);

                    IdentityRole identityRole = await _roleManager.Roles.Where(t => t.Id == model.RoleId).FirstOrDefaultAsync();
                    await _userManager.AddToRoleAsync(user, identityRole.Name);

                    return Ok(new { Status = "Success", User = _user });
                }

                return StatusCode(400, new { Status = "Error", Message = "Qaqa problem var!" });

                //return Ok(user);


            }
            return StatusCode(400, new { Status = "Error", Message = "Qaqa duz yaz!" });
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var a = await _userManager.GetRolesAsync(user);
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Id),
                    new Claim(ClaimTypes.Role, a.FirstOrDefault()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddHours(3),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }
            return Unauthorized();
        }

        // PUT api/<UserController>/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Human Resources")]
        public async Task<ActionResult> Put(string id, [FromBody] UserUpdateModel model)
        {
            string id2 = User.FindFirst(ClaimTypes.Name).Value;

            if (model == null)
            {
                return StatusCode(404, new { Status = "Error", Message = "Model is incorrect!", ModelState.ValidationState });
            }

            if (!_userManager.Users.Any(p => p.Id == id) && !_userManager.Users.Any(p => p.Id == id2))
            {
                return StatusCode(401, new { Status = "Error", Message = "There is not a such user!!" });
            }
            if (!_context.Positions.Any(p => p.Id == model.PositionId))
            {
                return StatusCode(401, new { Status = "Error", Message = "Qaqa beyle bir position yoxdu!" });
            }

            var user2 = await _context.Users.Include(t=>t.Position).ThenInclude(p=>p.Company).Where(u=>u.Id==id2).FirstOrDefaultAsync();
            var user3 = await _context.Users.Include(t => t.Position).ThenInclude(p => p.Company).Where(u => u.Id == id).FirstOrDefaultAsync();

            List<Position> positions = await _context.Positions.Where(p => p.CompanyId == user2.Position.CompanyId).ToListAsync();

            if (user2.Position.CompanyId != user3.Position.CompanyId) {
                return StatusCode(404, new { Status = "Error", Message = "You dont have permission to update this user!" });
            };

            if (!positions.Any(p=>p.Id==model.PositionId))
            {
                return StatusCode(404, new { Status = "Error", Message = "You dont have permission to use this Position", Model = model });
            }

            if (_roleManager.Roles.Where(r=>r.Id==model.RoleId)==null)
            {
                return StatusCode(404, new { Status = "Error", Message = "Such a Role doesnt exist!", Model = model });
            }

            if (ModelState.IsValid)
            {
                var user = await _context.Users.FindAsync(id);

                user.Name = model.Name;
                user.Surname = model.Surname;
                user.BDate = model.BDate;
                user.EmploymentDate = model.EmploymentDate;
                user.Email = model.Mail;
                user.PositionId = model.PositionId;
                user.CreatedAt = DateTime.Now;
                user.Status = true;
                user.UserName = model.Mail;
                user.PasswordHash = model.Password;

                _context.Users.Update(user);

                await _userManager.RemovePasswordAsync(user);
                await _userManager.AddPasswordAsync(user, model.Password);

                await _context.SaveChangesAsync();

                var rolesList = await _userManager.GetRolesAsync(user).ConfigureAwait(false);

                await _userManager.RemoveFromRoleAsync(user, rolesList.SingleOrDefault());
                await _userManager.AddToRoleAsync(user, _roleManager.Roles.Where(r => r.Id == model.RoleId).FirstOrDefault().Name);

                await _context.SaveChangesAsync();

                var user4 = await _context.Users.Include(u => u.Position).ThenInclude(p => p.Company).Where(u => u.Id == user.Id).FirstOrDefaultAsync();

                return StatusCode(200, new { Status = "Success", Message = "Qaqa zor!", User = new { name = user4.Name, surname = user4.Surname, birthday = user4.BDate, email = user4.Email, status = user4.Status, employmentDate = user4.EmploymentDate, createdDate = user4.CreatedAt, positions = user4.Position.PositionName, company = user4.Position.Company.CompanyName } });

            }

            return StatusCode(400, new { Status = "Error", Message = "Qaqa duz yaz!", ModelState.ValidationState});

        }

        // DELETE api/<UserController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        // DELETE api/<UserController>/5
        [HttpGet("get-user-info/{id}")]
        [Authorize(Roles = "Admin, Human Resources, Director")]
        public ActionResult GetUserInfo(string id)
        {
            if (_context.Users.Any(u => u.Id == id))
            {
                string role = User.FindFirst(ClaimTypes.Role).Value;
                HelperClass helperClass = new();

                if (role=="Admin")
                {
                    User user = _context.Users.Include(u => u.Position).ThenInclude(p => p.Company).Where(u => u.Id == id).FirstOrDefault();
                    List<Tax> taxes = _context.Taxes.Where(t => t.Type == user.Position.Company.CompanyType).ToList();

                    return Ok(new { Status = "Success", User = helperClass.GetUser(taxes, user, null) });
                }
                else
                {
                    string id2 = User.FindFirst(ClaimTypes.Name).Value;

                    User user1 = _context.Users.Include(u => u.Position).ThenInclude(p => p.Company).Where(u=>u.Id==id2).FirstOrDefault();
                    User user = _context.Users.Include(u => u.Position).ThenInclude(p => p.Company).Where(u => u.Id == id && u.Position.CompanyId == user1.Position.CompanyId).FirstOrDefault();
                    List<Tax> taxes = _context.Taxes.Where(t => t.Type == user.Position.Company.CompanyType).ToList();

                    return Ok(new { Status = "Success", User = helperClass.GetUser(taxes, user, null) });
                }

            }
            return StatusCode(404, new { Status = "Error", Message = "Something went wrong!" });
        }

        [HttpGet("get-exel")]
        public ActionResult GetExel()
        {
            List<User> users = _context.Users.Include(u => u.Position).ThenInclude(p => p.Company).ToList();
            List<Tax> taxes = _context.Taxes.ToList();
            HelperClass helperClass = new();
            List<AllUsersDetailsDTO> a = helperClass.GetUser(taxes, null, users) as List<AllUsersDetailsDTO>;

            if (a == null)
            {
                return BadRequest();
            }
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("sl1");

            ws.Row(1).Height = 5;
            ws.Row(2).Height = 30;
            ws.Row(3).Height = 18;

            ws.Column("A").Width = 5;
            ws.Column("B").Width = 15;
            ws.Column("C").Width = 15;
            ws.Column("D").Width = 30;
            ws.Column("E").Width = 15;
            ws.Column("F").Width = 15;
            ws.Column("G").Width = 20;
            ws.Column("H").Width = 20;
            ws.Column("I").Width = 15;
            ws.Column("J").Width = 15;

            ws.Range("A2:J2").Merge();
            ws.Range("A2:J2").Value = "User list";
            ws.Range("A2:J2").Style.Font.FontSize = 14;
            ws.Range("A2:J2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range("A2:J2").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Range("A2:J2").Style.Font.Bold = true;

            ws.Cell("A3").Value = "#";
            ws.Cell("A3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("A3").Style.Font.FontColor = XLColor.White;
            ws.Cell("A3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("A3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell("A3").Style.Font.Bold = true;
            ws.Cell("A3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("A3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("A3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("A3").Style.Border.RightBorder = XLBorderStyleValues.Thin;

            ws.Cell("B3").Value = "Name";
            ws.Cell("B3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("B3").Style.Font.FontColor = XLColor.White;
            ws.Cell("B3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("B3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell("B3").Style.Font.Bold = true;
            ws.Cell("B3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("B3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("B3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("B3").Style.Border.RightBorder = XLBorderStyleValues.Thin;

            ws.Cell("C3").Value = "Surname";
            ws.Cell("C3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("C3").Style.Font.FontColor = XLColor.White;
            ws.Cell("C3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("C3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell("C3").Style.Font.Bold = true;
            ws.Cell("C3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("C3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("C3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("C3").Style.Border.RightBorder = XLBorderStyleValues.Thin;

            ws.Cell("D3").Value = "Email";
            ws.Cell("D3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("D3").Style.Font.FontColor = XLColor.White;
            ws.Cell("D3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("D3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell("D3").Style.Font.Bold = true;
            ws.Cell("D3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("D3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("D3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("D3").Style.Border.RightBorder = XLBorderStyleValues.Thin;

            ws.Cell("E3").Value = "Created Date";
            ws.Cell("E3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("E3").Style.Font.FontColor = XLColor.White;
            ws.Cell("E3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("E3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell("E3").Style.Font.Bold = true;
            ws.Cell("E3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("E3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("E3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("E3").Style.Border.RightBorder = XLBorderStyleValues.Thin;

            ws.Cell("F3").Value = "Date of Bith";
            ws.Cell("F3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("F3").Style.Font.FontColor = XLColor.White;
            ws.Cell("F3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("F3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell("F3").Style.Font.Bold = true;
            ws.Cell("F3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("F3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("F3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("F3").Style.Border.RightBorder = XLBorderStyleValues.Thin;

            ws.Cell("G3").Value = "Company";
            ws.Cell("G3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("G3").Style.Font.FontColor = XLColor.White;
            ws.Cell("G3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("G3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell("G3").Style.Font.Bold = true;
            ws.Cell("G3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("G3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("G3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("G3").Style.Border.RightBorder = XLBorderStyleValues.Thin;

            ws.Cell("H3").Value = "Position";
            ws.Cell("H3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("H3").Style.Font.FontColor = XLColor.White;
            ws.Cell("H3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("H3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell("H3").Style.Font.Bold = true;
            ws.Cell("H3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("H3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("H3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("H3").Style.Border.RightBorder = XLBorderStyleValues.Thin;

            ws.Cell("I3").Value = "Net Salary";
            ws.Cell("I3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("I3").Style.Font.FontColor = XLColor.White;
            ws.Cell("I3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("I3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell("I3").Style.Font.Bold = true;
            ws.Cell("I3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("I3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("I3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("I3").Style.Border.RightBorder = XLBorderStyleValues.Thin;

            ws.Cell("J3").Value = "Gross Salary";
            ws.Cell("J3").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
            ws.Cell("J3").Style.Font.FontColor = XLColor.White;
            ws.Cell("J3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("J3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell("J3").Style.Font.Bold = true;
            ws.Cell("J3").Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            ws.Cell("J3").Style.Border.TopBorder = XLBorderStyleValues.Thin;
            ws.Cell("J3").Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            ws.Cell("J3").Style.Border.RightBorder = XLBorderStyleValues.Thin;

            for (int i = 0; i < a.Count; i++)
            {
                ws.Cell("A" + (i + 4)).Value = i + 1;
                ws.Cell("A" + (i + 4)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell("A" + (i + 4)).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell("A" + (i + 4)).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("A" + (i + 4)).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("A" + (i + 4)).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("A" + (i + 4)).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                ws.Cell("B" + (i + 4)).Value = a[i].Name;
                ws.Cell("B" + (i + 4)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                ws.Cell("B" + (i + 4)).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell("B" + (i + 4)).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + (i + 4)).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + (i + 4)).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("B" + (i + 4)).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                ws.Cell("C" + (i + 4)).Value = a[i].Surname;
                ws.Cell("C" + (i + 4)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell("C" + (i + 4)).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell("C" + (i + 4)).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + (i + 4)).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + (i + 4)).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("C" + (i + 4)).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                ws.Cell("D" + (i + 4)).Value = a[i].Email;
                ws.Cell("D" + (i + 4)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell("D" + (i + 4)).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell("D" + (i + 4)).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + (i + 4)).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + (i + 4)).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("D" + (i + 4)).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                ws.Cell("E" + (i + 4)).Value = a[i].CreatedDAte.ToString("dd.MM.yyyy");
                ws.Cell("E" + (i + 4)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell("E" + (i + 4)).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell("E" + (i + 4)).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("E" + (i + 4)).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("E" + (i + 4)).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("E" + (i + 4)).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                ws.Cell("F" + (i + 4)).Value = a[i].Bdate.ToString("dd.MM.yyyy");
                ws.Cell("F" + (i + 4)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell("F" + (i + 4)).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell("F" + (i + 4)).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("F" + (i + 4)).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("F" + (i + 4)).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("F" + (i + 4)).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                ws.Cell("G" + (i + 4)).Value = a[i].Company;
                ws.Cell("G" + (i + 4)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell("G" + (i + 4)).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell("G" + (i + 4)).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("G" + (i + 4)).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("G" + (i + 4)).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("G" + (i + 4)).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                ws.Cell("H" + (i + 4)).Value = a[i].Position;
                ws.Cell("H" + (i + 4)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell("H" + (i + 4)).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell("H" + (i + 4)).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("H" + (i + 4)).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("H" + (i + 4)).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("H" + (i + 4)).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                ws.Cell("I" + (i + 4)).Value = a[i].NettSalary;
                ws.Cell("I" + (i + 4)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell("I" + (i + 4)).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell("I" + (i + 4)).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("I" + (i + 4)).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("I" + (i + 4)).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("I" + (i + 4)).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                ws.Cell("J" + (i + 4)).Value = a[i].GrossSalary;
                ws.Cell("J" + (i + 4)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell("J" + (i + 4)).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell("J" + (i + 4)).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cell("J" + (i + 4)).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cell("J" + (i + 4)).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cell("J" + (i + 4)).Style.Border.RightBorder = XLBorderStyleValues.Thin;
            }

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            var content = stream.ToArray();
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Users.xlsx");
        }
    }
}
