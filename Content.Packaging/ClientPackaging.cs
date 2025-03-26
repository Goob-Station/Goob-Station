using System.Diagnostics;
using System.IO.Compression;
using Robust.Packaging;
using Robust.Packaging.AssetProcessing;
using Robust.Packaging.AssetProcessing.Passes;
using Robust.Packaging.Utility;
using Robust.Shared.Timing;

namespace Content.Packaging;

public static class ClientPackaging
{
    /// <summary>
    /// Be advised this can be called from server packaging during a HybridACZ build.
    /// Be also advised this goes against god and nature
    /// </summary>
    public static async Task PackageClient(bool skipBuild, string configuration, IPackageLogger logger, string path = ".")
    {
        logger.Info("Building client...");

        if (!skipBuild)
        {
            var clientProjects = GetClientModules(path);

            foreach (var project in clientProjects)
            {
                await ProcessHelpers.RunCheck(new ProcessStartInfo
                {
                    FileName = "dotnet",
                    ArgumentList =
                    {
                        "build",
                        project,
                        "-c", configuration,
                        "--nologo",
                        "/v:m",
                        "/t:Rebuild",
                        "/p:FullRelease=true",
                        "/m"
                    }
                });
            }
        }

        logger.Info("Packaging client...");

        var sw = RStopwatch.StartNew();
        {
            await using var zipFile =
                File.Open(Path.Combine("release", "SS14.Client.zip"), FileMode.Create, FileAccess.ReadWrite);
            using var zip = new ZipArchive(zipFile, ZipArchiveMode.Update);
            var writer = new AssetPassZipWriter(zip);

            await WriteResources("", writer, logger, default);
            await writer.FinishedTask;
        }

        logger.Info($"Finished packaging client in {sw.Elapsed}");
    }

    private static List<string> GetClientModules(string path)
    {
        var clientProjects = new List<string> { Path.Combine("Content.Client", "Content.Client.csproj") };

        var directories = Directory.GetDirectories(path, "Content.*");

        foreach (var dir in directories)
        {
            var dirName = Path.GetFileName(dir);

            if (dirName != "Content.Client" && dirName.EndsWith(".Client"))
            {
                var projectPath = Path.Combine(dir, $"{dirName}.csproj");
                if (File.Exists(projectPath))
                {
                    clientProjects.Add(projectPath);
                }
            }
        }

        return clientProjects;
    }

    private static List<string> FindAllModules(string path = ".")
    {
        // Correct pathing to be in local folder if contentDir is empty.
        if (string.IsNullOrEmpty(path))
            path = ".";

        var modules = new List<string> { "Content.Client", "Content.Shared", "Content.Shared.Database" };

        var directories = Directory.GetDirectories(path, "Content.*");
        foreach (var dir in directories)
        {
            var dirName = Path.GetFileName(dir);

            // Throw in anything that ends with ".Client", ".Shared" or ".Common"
            if ((dirName.EndsWith(".Client") || dirName.EndsWith(".Shared") || dir.EndsWith(".Common")) &&
                !modules.Contains(dirName))
            {
                var projectPath = Path.Combine(dir, $"{dirName}.csproj");
                if (File.Exists(projectPath))
                {
                    modules.Add(dirName);
                }
            }
        }

        return modules;
    }

    public static async Task WriteResources(
        string contentDir,
        AssetPass pass,
        IPackageLogger logger,
        CancellationToken cancel)
    {
        var graph = new RobustClientAssetGraph();
        pass.Dependencies.Add(new AssetPassDependency(graph.Output.Name));

        var dropSvgPass = new AssetPassFilterDrop(f => f.Path.EndsWith(".svg"))
        {
            Name = "DropSvgPass",
        };
        dropSvgPass.AddDependency(graph.Input).AddBefore(graph.PresetPasses);

        AssetGraph.CalculateGraph([pass, dropSvgPass, ..graph.AllPasses], logger);

        var inputPass = graph.Input;

        var modules = FindAllModules(contentDir);

        await RobustSharedPackaging.WriteContentAssemblies(
            inputPass,
            contentDir,
            "Content.Client",
            modules.ToArray(),
            cancel: cancel);

        await RobustClientPackaging.WriteClientResources(contentDir, inputPass, cancel);

        inputPass.InjectFinished();
    }
}
