using Microsoft.AspNetCore.Mvc;
using SkillSnap.Api.Data;
using SkillSnap.Api.Models;
namespace SkillSnap.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GetController : ControllerBase
    {
        private readonly SkillSnapContext _context;
        public GetController(SkillSnapContext context)
        {
            _context = context;
        }
       
        [HttpGet]
        public IActionResult Get()
        {
            var users = _context.PortfolioUsers.ToList();
            return Ok(users);
        }
    }
}