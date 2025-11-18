// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Vasilis <vasilis@pikachu.systems>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics;
using System.IO.Compression;
using Content.ModuleManager;
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

        // Modules - Add modules from Modules/ directory
        clientProjects.AddRange(
            ModuleDiscovery.DiscoverModules(path)
                .Where(m => m.Type == ModuleRole.Client)
                .Select(m => m.ProjectPath)
        );

        return clientProjects;
    }

    private static List<string> FindAllModules(string path = ".")
    {
        // Correct pathing to be in local folder if contentDir is empty.
        if (string.IsNullOrEmpty(path))
            path = ".";

        var modules = new List<string> { "Content.Client", "Content.Shared", "Content.Shared.Database", "Content.ModuleManager" };

        // Modules - Add modules from Modules/ directory
        modules.AddRange(
            ModuleDiscovery.DiscoverModules(path)
                .Where(m => m.Type != ModuleRole.Server)
                .Select(m => m.Name)
                .Distinct()
        );

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

        await WriteClientContentAssemblies(
            inputPass,
            contentDir,
            modules,
            cancel);

        await RobustClientPackaging.WriteClientResources(contentDir, inputPass, cancel);

        inputPass.InjectFinished();
    }

    private static Task WriteClientContentAssemblies(
        AssetPass pass,
        string contentDir,
        IEnumerable<string> contentAssemblies,
        CancellationToken cancel = default)
    {
        var mainBinDir = Path.Combine(contentDir, "bin", "Content.Client");

        var moduleAssemblyPaths = ModuleDiscovery.DiscoverModules(contentDir)
            .Where(m => m.Type == ModuleRole.Client)
            .ToDictionary(
                m => m.Name,
                m => Path.Combine(GetModuleRoot(m.ProjectPath), "bin", "Content.Client")
            );

        foreach (var asm in contentAssemblies)
        {
            cancel.ThrowIfCancellationRequested();

            var sourceDir = moduleAssemblyPaths.GetValueOrDefault(asm) ?? mainBinDir;

            var dllPath = Path.Combine(sourceDir, $"{asm}.dll");
            if (File.Exists(dllPath))
                pass.InjectFileFromDisk($"Assemblies/{asm}.dll", dllPath);

            var pdbPath = Path.Combine(sourceDir, $"{asm}.pdb");
            if (File.Exists(pdbPath))
                pass.InjectFileFromDisk($"Assemblies/{asm}.pdb", pdbPath);
        }

        return Task.CompletedTask;
    }

    private static string GetModuleRoot(string projectPath)
    {
        // Extracts the module root from the project path
        // e.g., "Modules/GoobStation/Content.Goobstation.Client/Content.Goobstation.Client.csproj"
        // -> "Modules/GoobStation"
        var projectDir = Path.GetDirectoryName(projectPath);
        return Path.GetDirectoryName(projectDir)!;
    }
}
