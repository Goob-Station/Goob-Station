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
using Robust.Packaging;
using Robust.Packaging.AssetProcessing;
using Robust.Packaging.AssetProcessing.Passes;
using Robust.Packaging.Utility;
using Robust.Shared.Timing;

namespace Content.Packaging;

public static class ClientPackaging
{
    private static readonly bool UseSecrets = File.Exists(Path.Combine("Secrets", "CorvaxSecrets.sln")); // CorvaxGoob-Secrets

    /// <summary>
    /// Be advised this can be called from server packaging during a HybridACZ build.
    /// Be also advised this goes against god and nature
    /// </summary>
    public static async Task PackageClient(bool skipBuild, string configuration, IPackageLogger logger)
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
                    Path.Combine("Content.Client", "Content.Client.csproj"),
                    "-c", configuration,
                    "--nologo",
                    "/v:m",
                    "/t:Rebuild",
                    "/p:FullRelease=true",
                    "/m"
                }
            });
            if (UseSecrets)
            {
                await ProcessHelpers.RunCheck(new ProcessStartInfo
                {
                    FileName = "dotnet",
                    ArgumentList =
                    {
                        "build",
                        Path.Combine("Secrets","Content.Corvax.Client", "Content.Corvax.Client.csproj"),
                        "-c", "Release",
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

        AssetGraph.CalculateGraph([pass, dropSvgPass, .. graph.AllPasses], logger);

        var inputPass = graph.Input;

        // CorvaxGoob-Secrets-Start: Add Corvax interfaces to Magic ACZ
        var assemblies = new List<string> { "Content.Client", "Content.Shared", "Content.Shared.Database", "Content.Corvax.Interfaces.Client", "Content.Corvax.Interfaces.Shared" };
        if (UseSecrets)
            assemblies.AddRange(new[] { "Content.Corvax.Shared", "Content.Corvax.Client" });
        // CorvaxGoob-Secrets-End

        await RobustSharedPackaging.WriteContentAssemblies(
            inputPass,
            contentDir,
            "Content.Client",
            assemblies, // CorvaxGoob-Secrets
            cancel: cancel);

        await WriteClientResources(contentDir, inputPass, cancel); // CorvaxGoob-Secrets: Support content resource ignore to ignore server-only prototypes

        inputPass.InjectFinished();
    }

    // CorvaxGoob-Secrets-Start
    public static IReadOnlySet<string> ContentClientIgnoredResources { get; } = new HashSet<string>
    {
        "CorvaxSecretsServer"
    };

    private static async Task WriteClientResources(
        string contentDir,
        AssetPass pass,
        CancellationToken cancel = default)
    {
        var ignoreSet = RobustClientPackaging.ClientIgnoredResources
            .Union(RobustSharedPackaging.SharedIgnoredResources)
            .Union(ContentClientIgnoredResources).ToHashSet();

        await RobustSharedPackaging.DoResourceCopy(Path.Combine(contentDir, "Resources"), pass, ignoreSet, cancel: cancel);
    }
    // CorvaxGoob-Secrets-End
}
