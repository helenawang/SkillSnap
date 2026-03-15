using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Api.Data;
using SkillSnap.Api.Models;

namespace SkillSnap.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SkillsController : ControllerBase
{
    private readonly SkillSnapContext _context;

    public SkillsController(SkillSnapContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    // GET: /api/skills
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Skill>>> GetSkills()
    {
        var skills = await _context.Set<Skill>()
            .AsNoTracking()
            .ToListAsync();

        return Ok(skills);
    }

    // GET: /api/skills/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Skill>> GetSkillById(int id)
    {
        var skill = await _context.Set<Skill>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (skill is null) return NotFound();
        return Ok(skill);
    }

    // POST: /api/skills
    [HttpPost]
    public async Task<ActionResult<Skill>> CreateSkill([FromBody] Skill skill)
    {
        if (string.IsNullOrWhiteSpace(skill.Name))
            return BadRequest("Name is required.");

        var userExists = await _context.Set<PortfolioUser>()
            .AnyAsync(u => u.Id == skill.PortfolioUserId);

        if (!userExists)
            return BadRequest("Invalid PortfolioUserId.");

        _context.Set<Skill>().Add(skill);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSkillById), new { id = skill.Id }, skill);
    }
}