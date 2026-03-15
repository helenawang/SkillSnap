using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillSnap.Api.Models;

public class Skill
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public int PortfolioUserId { get; set; }

    // navigational property
    [ForeignKey(nameof(PortfolioUserId))]
    public PortfolioUser PortfolioUser { get; set; } = null!;
}