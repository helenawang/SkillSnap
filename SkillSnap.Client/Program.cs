using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SkillSnap.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

await builder.Build().RunAsync();

builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<SkillService>();