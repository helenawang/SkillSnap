using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SkillSnap.Api.Data;
using SkillSnap.Api.Models;

namespace SkillSnap.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly SkillSnapContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ProjectsController> _logger;

    private static readonly TimeSpan PrimaryAbsoluteExpiration = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan PrimarySlidingExpiration = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan FallbackAbsoluteExpiration = TimeSpan.FromMinutes(30);

    public ProjectsController(
        SkillSnapContext context,
        IMemoryCache cache,
        ILogger<ProjectsController> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // GET: /api/projects
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
    {
        const string cacheKey = "projects:all";
        const string fallbackKey = "projects:all:fallback";

        if (_cache.TryGetValue(cacheKey, out List<Project>? cachedProjects) && cachedProjects is not null)
            return Ok(cachedProjects);

        try
        {
            var projects = await _context.Set<Project>()
                .AsNoTracking()
                .Include(p => p.PortfolioUser)
                .ToListAsync();

            _cache.Set(cacheKey, projects, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = PrimaryAbsoluteExpiration,
                SlidingExpiration = PrimarySlidingExpiration
            });

            _cache.Set(fallbackKey, projects, FallbackAbsoluteExpiration);

            return Ok(projects);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read projects from DB. Attempting fallback cache.");

            if (_cache.TryGetValue(fallbackKey, out List<Project>? fallbackProjects) && fallbackProjects is not null)
                return Ok(fallbackProjects);

            return StatusCode(StatusCodes.Status503ServiceUnavailable, "Projects are temporarily unavailable.");
        }
    }

    // POST: /api/projects
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<Project>> CreateProject([FromBody] Project project)
    {
        if (project is null)
            return BadRequest("Project payload is required.");

        if (string.IsNullOrWhiteSpace(project.Title))
            return BadRequest("Title is required.");

        var userExists = await _context.Set<PortfolioUser>()
            .AsNoTracking()
            .AnyAsync(u => u.Id == project.PortfolioUserId);

        if (!userExists)
            return BadRequest("Invalid PortfolioUserId.");

        _context.Set<Project>().Add(project);
        await _context.SaveChangesAsync();

        _cache.Remove("projects:all");
        _cache.Remove("projects:all:fallback");

        var itemKey = $"projects:{project.Id}";
        var itemFallbackKey = $"projects:{project.Id}:fallback";

        _cache.Set(itemKey, project, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = PrimaryAbsoluteExpiration,
            SlidingExpiration = PrimarySlidingExpiration
        });
        _cache.Set(itemFallbackKey, project, FallbackAbsoluteExpiration);

        return CreatedAtAction(nameof(GetProjectById), new { id = project.Id }, project);
    }

    // GET: /api/projects/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Project>> GetProjectById(int id)
    {
        var cacheKey = $"projects:{id}";
        var fallbackKey = $"projects:{id}:fallback";

        if (_cache.TryGetValue(cacheKey, out Project? cachedProject) && cachedProject is not null)
            return Ok(cachedProject);

        try
        {
            var project = await _context.Set<Project>()
                .AsNoTracking()
                .Include(p => p.PortfolioUser)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project is null) return NotFound();

            _cache.Set(cacheKey, project, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = PrimaryAbsoluteExpiration,
                SlidingExpiration = PrimarySlidingExpiration
            });

            _cache.Set(fallbackKey, project, FallbackAbsoluteExpiration);

            return Ok(project);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read project {ProjectId} from DB. Attempting fallback cache.", id);

            if (_cache.TryGetValue(fallbackKey, out Project? fallbackProject) && fallbackProject is not null)
                return Ok(fallbackProject);

            return StatusCode(StatusCodes.Status503ServiceUnavailable, "Project is temporarily unavailable.");
        }
    }
}