// SPDX-License-Identifier: AGPL-3.0-or-later

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
            await ProcessHelpers.RunCheck(new ProcessStartInfo
            {
                FileName = "dotnet",
                ArgumentList =
                {
                    "build",
                    Path.Combine("Content.Goobstation.Client", "Content.Goobstation.Client.csproj"), // Goob
                    "-c", configuration,
                    "--nologo",
                    "/v:m",
                    "/t:Rebuild",
                    "/p:FullRelease=true",
                    "/m"
                }
            });
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

        // Goob edit start
        // thanks 'dletandas'
        var sourcePath = Path.Combine(contentDir, "bin", "Content.Client");
        var deps = DepsHandler.Load(Path.Combine(sourcePath, "Content.Goobstation.Client.deps.json"));
        var contentAssemblies = ServerPackaging.GetContentAssemblyNamesToCopy(deps, "Client");
        // Good edit end

        await RobustSharedPackaging.WriteContentAssemblies(
            inputPass,
            contentDir,
            "Content.Client",
            contentAssemblies, // Goob edit
            cancel: cancel);

        await RobustClientPackaging.WriteClientResources(
            contentDir,
            inputPass,
            SharedPackaging.AdditionalIgnoredResources,
            cancel);

        inputPass.InjectFinished();
    }
}
