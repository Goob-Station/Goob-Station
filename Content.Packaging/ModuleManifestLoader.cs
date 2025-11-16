using Content.ModuleManager;
using Robust.Shared.Serialization.Markdown;
using Robust.Shared.Serialization.Markdown.Mapping;
using Robust.Shared.Serialization.Markdown.Sequence;
using Robust.Shared.Serialization.Markdown.Value;

namespace Content.Packaging;

public static class ModuleManifestLoader
{
    /// <summary>
    /// Loads a module manifest from a module.yml file
    /// </summary>
    public static ModuleManifest LoadFromFile(string manifestPath)
    {
        if (!File.Exists(manifestPath))
            throw new FileNotFoundException($"Module manifest not found: {manifestPath}");

        using var reader = File.OpenText(manifestPath);
        var documents = DataNodeParser.ParseYamlStream(reader);
        var document = documents.FirstOrDefault();

        if (document?.Root is not MappingDataNode root)
            throw new InvalidDataException($"Invalid module manifest format: {manifestPath}");

        var fullPath = Path.GetFullPath(manifestPath);
        var manifest = new ModuleManifest
        {
            ManifestPath = fullPath,
            ModuleDirectory = Path.GetDirectoryName(fullPath) ?? string.Empty
        };

        manifest.Name = GetRequiredString(root, "name", manifestPath);
        manifest.Id = GetRequiredString(root, "id", manifestPath);
        manifest.Version = GetRequiredString(root, "version", manifestPath);

        if (!IsValidId(manifest.Id))
            throw new InvalidDataException($"Invalid module ID '{manifest.Id}' in {manifestPath}. Must be lowercase alphanumeric with underscores only.");

        if (!root.TryGet("projects", out var projectsNode) || projectsNode is not SequenceDataNode projectsSeq)
            throw new InvalidDataException($"Module manifest must contain 'projects' list: {manifestPath}");

        manifest.Projects = ParseProjects(projectsSeq, manifestPath);

        if (manifest.Projects.Count == 0)
            throw new InvalidDataException($"Module manifest must contain at least one project: {manifestPath}");

        return manifest;
    }

    /// <summary>
    /// Gets the full path to a project's .csproj file
    /// </summary>
    public static string GetProjectPath(ProjectInfo project, string moduleDirectory)
    {
        var projectDir = Path.Combine(moduleDirectory, project.Path);
        var projectName = Path.GetFileName(projectDir);
        return Path.Combine(projectDir, $"{projectName}.csproj");
    }

    /// <summary>
    /// Gets just the project directory name (for the assembly name)
    /// </summary>
    public static string GetProjectName(ProjectInfo project)
    {
        return Path.GetFileName(project.Path);
    }

    private static List<ProjectInfo> ParseProjects(SequenceDataNode projectsSeq, string manifestPath)
    {
        var projects = new List<ProjectInfo>();

        for (int i = 0; i < projectsSeq.Count; i++)
        {
            if (projectsSeq[i] is not MappingDataNode projectNode)
                throw new InvalidDataException($"Project entry {i} must be a mapping in {manifestPath}");

            var project = new ProjectInfo
            {
                Path = GetRequiredString(projectNode, "path", manifestPath),
                Role = ParseRole(GetRequiredString(projectNode, "role", manifestPath), manifestPath)
            };

            projects.Add(project);
        }

        return projects;
    }

    private static ModuleRole ParseRole(string roleStr, string manifestPath)
    {
        if (Enum.TryParse<ModuleRole>(roleStr, ignoreCase: true, out var role))
            return role;

        throw new InvalidDataException($"Invalid role '{roleStr}' in {manifestPath}. Must be Client, Server, Shared, or Common.");
    }

    private static string GetRequiredString(MappingDataNode node, string key, string manifestPath)
    {
        if (!node.TryGet(key, out var valueNode) || valueNode is not ValueDataNode value)
            throw new InvalidDataException($"Missing required field '{key}' in {manifestPath}");

        if (string.IsNullOrWhiteSpace(value.Value))
            throw new InvalidDataException($"Field '{key}' cannot be empty in {manifestPath}");

        return value.Value;
    }

    private static bool IsValidId(string id)
    {
        return !string.IsNullOrEmpty(id)
            && id.All(c => char.IsLower(c) || char.IsDigit(c) || c == '_')
            && char.IsLetter(id[0]);
    }
}
