using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Api.Data;
using SkillSnap.Api.Models;

namespace SkillSnap.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly SkillSnapContext _context;

    public ProjectsController(SkillSnapContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    // GET: /api/projects
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
    {
        var projects = await _context.Set<Project>()
            .AsNoTracking()
            .ToListAsync();

        return Ok(projects);
    }

    // POST: /api/projects
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<Project>> CreateProject([FromBody] Project project)
    {
        if (string.IsNullOrWhiteSpace(project.Title))
            return BadRequest("Title is required.");

        // Optional: ensure FK points to an existing user
        var userExists = await _context.Set<PortfolioUser>()
            .AnyAsync(u => u.Id == project.PortfolioUserId);

        if (!userExists)
            return BadRequest("Invalid PortfolioUserId.");

        _context.Set<Project>().Add(project);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProjectById), new { id = project.Id }, project);
    }

    // GET: /api/projects/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Project>> GetProjectById(int id)
    {
        var project = await _context.Set<Project>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project is null) return NotFound();

        return Ok(project);
    }
}