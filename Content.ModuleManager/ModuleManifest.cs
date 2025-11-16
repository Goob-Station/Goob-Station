namespace Content.ModuleManager;

public sealed class ModuleManifest
{
    public string Name { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public List<ProjectInfo> Projects { get; set; } = new();

    public string ManifestPath { get; set; } = string.Empty;

    public string ModuleDirectory { get; set; } = string.Empty;
}

public sealed class ProjectInfo
{
    public string Path { get; set; } = string.Empty;
    public ModuleRole Role { get; set; }
}

public enum ModuleRole
{
    Client,  // Client-only code
    Server,  // Server-only code
    Shared,  // Shared between client and server
    Common   // Common code (Does not reference anything other than RobustToolbox)
}
