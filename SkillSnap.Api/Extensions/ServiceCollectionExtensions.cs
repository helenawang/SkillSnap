using Microsoft.EntityFrameworkCore;
using SkillSnap.Api.Data;

namespace SkillSnap.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddRazorComponents()
            .AddInteractiveWebAssemblyComponents();

        services.AddDbContext<SkillSnapContext>(options =>
            options.UseSqlite(config.GetConnectionString("DefaultConnection")));

        services.AddControllers();

        return services;
    }
}