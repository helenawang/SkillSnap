using System.Net.Http.Json;
using SkillSnap.Shared.Models;

namespace SkillSnap.Client.Services;

public class SkillService
{
    private readonly HttpClient _httpClient;

    public SkillService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<SkillDto>> GetSkillsAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<List<SkillDto>>("/api/skills");
        return result ?? new List<SkillDto>();
    }

    public async Task<SkillDto?> AddSkillAsync(SkillDto newSkill)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/skills", newSkill);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<SkillDto>();
    }
}