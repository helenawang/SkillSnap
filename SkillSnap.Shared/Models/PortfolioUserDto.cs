namespace SkillSnap.Shared.Models;

public class PortfolioUserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string ProfileImageUrl { get; set; } = string.Empty;
    public List<ProjectDto> Projects { get; set; } = new();
    public List<SkillDto> Skills { get; set; } = new();
}
