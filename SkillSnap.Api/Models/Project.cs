using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillSnap.Api.Models;

public class Project
{
    [Key]
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int PortfolioUserId { get; set; }

    // navigational property
    [ForeignKey(nameof(PortfolioUserId))]
    public PortfolioUser PortfolioUser { get; set; } = null!;
}