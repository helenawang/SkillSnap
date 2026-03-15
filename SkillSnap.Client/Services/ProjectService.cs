using System.Net.Http.Json;
using SkillSnap.Shared.Models;

namespace SkillSnap.Client.Services;

public class ProjectService
{
    private readonly HttpClient _httpClient;

    public ProjectService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<ProjectDto>> GetProjectsAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<List<ProjectDto>>("/api/projects");
        return result ?? new List<ProjectDto>();
    }

    public async Task<ProjectDto?> AddProjectAsync(ProjectDto newProject)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/projects", newProject);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ProjectDto>();
    }
}