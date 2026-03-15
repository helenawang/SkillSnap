using Microsoft.EntityFrameworkCore;
using SkillSnap.Api.Models;

namespace SkillSnap.Api.Data;

public class SkillSnapContext : DbContext
{
    public SkillSnapContext(DbContextOptions<SkillSnapContext> options) : base(options) { }
    public DbSet<PortfolioUser> PortfolioUsers { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Skill> Skills { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PortfolioUser>()
            .HasMany(u => u.Projects)
            .WithOne(p => p.PortfolioUser)
            .HasForeignKey(p => p.PortfolioUserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PortfolioUser>()
            .HasMany(u => u.Skills)
            .WithOne(s => s.PortfolioUser)
            .HasForeignKey(s => s.PortfolioUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}