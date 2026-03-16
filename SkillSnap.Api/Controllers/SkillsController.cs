using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SkillSnap.Api.Data;
using SkillSnap.Api.Models;

namespace SkillSnap.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SkillsController : ControllerBase
{
    private readonly SkillSnapContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SkillsController> _logger;

    private static readonly TimeSpan PrimaryAbsoluteExpiration = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan PrimarySlidingExpiration = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan FallbackAbsoluteExpiration = TimeSpan.FromMinutes(30);

    public SkillsController(
        SkillSnapContext context,
        IMemoryCache cache,
        ILogger<SkillsController> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // GET: /api/skills
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Skill>>> GetSkills()
    {
        const string cacheKey = "skills:all";
        const string fallbackKey = "skills:all:fallback";

        if (_cache.TryGetValue(cacheKey, out List<Skill>? cachedSkills) && cachedSkills is not null)
            return Ok(cachedSkills);

        try
        {
            var skills = await _context.Set<Skill>()
                .AsNoTracking()
                .Include(s => s.PortfolioUser)
                .ToListAsync();

            _cache.Set(cacheKey, skills, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = PrimaryAbsoluteExpiration,
                SlidingExpiration = PrimarySlidingExpiration
            });

            _cache.Set(fallbackKey, skills, FallbackAbsoluteExpiration);

            return Ok(skills);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read skills from DB. Attempting fallback cache.");

            if (_cache.TryGetValue(fallbackKey, out List<Skill>? fallbackSkills) && fallbackSkills is not null)
                return Ok(fallbackSkills);

            return StatusCode(StatusCodes.Status503ServiceUnavailable, "Skills are temporarily unavailable.");
        }
    }

    // GET: /api/skills/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Skill>> GetSkillById(int id)
    {
        var cacheKey = $"skills:{id}";
        var fallbackKey = $"skills:{id}:fallback";

        if (_cache.TryGetValue(cacheKey, out Skill? cachedSkill) && cachedSkill is not null)
            return Ok(cachedSkill);

        try
        {
            var skill = await _context.Set<Skill>()
                .AsNoTracking()
                .Include(s => s.PortfolioUser)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (skill is null) return NotFound();

            _cache.Set(cacheKey, skill, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = PrimaryAbsoluteExpiration,
                SlidingExpiration = PrimarySlidingExpiration
            });

            _cache.Set(fallbackKey, skill, FallbackAbsoluteExpiration);

            return Ok(skill);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read skill {SkillId} from DB. Attempting fallback cache.", id);

            if (_cache.TryGetValue(fallbackKey, out Skill? fallbackSkill) && fallbackSkill is not null)
                return Ok(fallbackSkill);

            return StatusCode(StatusCodes.Status503ServiceUnavailable, "Skill is temporarily unavailable.");
        }
    }

    // POST: /api/skills
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<Skill>> CreateSkill([FromBody] Skill skill)
    {
        if (skill is null)
            return BadRequest("Skill payload is required.");

        if (string.IsNullOrWhiteSpace(skill.Name))
            return BadRequest("Name is required.");

        var userExists = await _context.Set<PortfolioUser>()
            .AsNoTracking()
            .AnyAsync(u => u.Id == skill.PortfolioUserId);

        if (!userExists)
            return BadRequest("Invalid PortfolioUserId.");

        _context.Set<Skill>().Add(skill);
        await _context.SaveChangesAsync();

        // Invalidate list caches
        _cache.Remove("skills:all");
        _cache.Remove("skills:all:fallback");

        // Prime item cache
        var itemKey = $"skills:{skill.Id}";
        var itemFallbackKey = $"skills:{skill.Id}:fallback";

        _cache.Set(itemKey, skill, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = PrimaryAbsoluteExpiration,
            SlidingExpiration = PrimarySlidingExpiration
        });
        _cache.Set(itemFallbackKey, skill, FallbackAbsoluteExpiration);

        return CreatedAtAction(nameof(GetSkillById), new { id = skill.Id }, skill);
    }
}