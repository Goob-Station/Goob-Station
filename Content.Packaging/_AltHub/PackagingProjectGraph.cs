// SPDX-FileCopyrightText: Yaroslav Yudaev <ydaevy10@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Xml.Linq;

namespace Content.Packaging;

internal static class PackagingProjectGraph
{
    private sealed record ProjectInfo(string Name, string ProjectPath, IReadOnlyCollection<string> References);

    public static List<string> GetLeafProjectPaths(string path, Func<string, bool> projectFilter)
    {
        var projects = GetProjects(path, projectFilter);

        if (projects.Count == 0)
            return new List<string>();

        var referencedProjects = new HashSet<string>(
            projects.SelectMany(project => project.References),
            StringComparer.OrdinalIgnoreCase);

        return projects
            .Where(project => !referencedProjects.Contains(project.Name))
            .Select(project => project.ProjectPath)
            .OrderBy(projectPath => projectPath, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static List<ProjectInfo> GetProjects(string path, Func<string, bool> projectFilter)
    {
        var projectPaths = Directory.GetDirectories(path, "Content.*")
            .Select(directory =>
            {
                var directoryName = Path.GetFileName(directory);
                var projectPath = Path.Combine(directory, $"{directoryName}.csproj");
                return new { directoryName, projectPath };
            })
            .Where(project => projectFilter(project.directoryName) && File.Exists(project.projectPath))
            .ToList();

        var knownProjectNames = projectPaths
            .Select(project => project.directoryName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return projectPaths
            .Select(project => new ProjectInfo(
                project.directoryName,
                project.projectPath,
                GetProjectReferences(project.projectPath, knownProjectNames)))
            .ToList();
    }

    private static IReadOnlyCollection<string> GetProjectReferences(string projectPath, IReadOnlySet<string> knownProjectNames)
    {
        var document = XDocument.Load(projectPath);
        var projectNamespace = document.Root?.Name.Namespace ?? XNamespace.None;
        var references = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var projectReference in document.Descendants(projectNamespace + "ProjectReference"))
        {
            var include = projectReference.Attribute("Include")?.Value;
            if (string.IsNullOrWhiteSpace(include))
                continue;

            var normalizedInclude = include.Replace('\\', Path.DirectorySeparatorChar);
            var referencedProjectPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(projectPath)!, normalizedInclude));
            var referencedProjectName = Path.GetFileNameWithoutExtension(referencedProjectPath);

            if (knownProjectNames.Contains(referencedProjectName))
                references.Add(referencedProjectName);
        }

        return references;
    }
}
