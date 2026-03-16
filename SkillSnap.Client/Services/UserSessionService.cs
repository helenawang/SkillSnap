namespace SkillSnap.Client.Services;

public sealed class UserSessionService
{
    public string? UserId { get; private set; }
    public string? Role { get; private set; }

    // Current editing/project state
    public int? CurrentProjectId { get; private set; }
    public bool IsEditingProject { get; private set; }

    public event Action? OnChange;

    public void SetUser(string? userId, string? role)
    {
        UserId = userId;
        Role = role;
        NotifyStateChanged();
    }

    public void SetProjectEditingState(int? projectId, bool isEditing)
    {
        CurrentProjectId = projectId;
        IsEditingProject = isEditing;
        NotifyStateChanged();
    }

    public void ClearProjectState()
    {
        CurrentProjectId = null;
        IsEditingProject = false;
        NotifyStateChanged();
    }

    public void ClearAll()
    {
        UserId = null;
        Role = null;
        CurrentProjectId = null;
        IsEditingProject = false;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}