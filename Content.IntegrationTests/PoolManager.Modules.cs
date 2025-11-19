// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Ted Lukin <66275205+pheenty@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Content.ModuleManager;
using Robust.Shared.ContentPack;

namespace Content.IntegrationTests;

public static partial class PoolManager
{
    // Modules that match ContentPrefix+suffix[..1] are considered "core" modules.
    // So, Content.Shared, Content.Client, Content.Server are "core" modules
    // Content.Common is not a thing by default but will be considered a core module if found.
    private static readonly string ContentPrefix = "Content.";
    private static readonly string[] Suffixes = [".Shared", ".Client", ".Server", ".Common", ".UIKit", ".Maths"];
    private static readonly Assembly CurrentAssembly = typeof(PoolManager).Assembly;

    private static readonly HashSet<Assembly> Client = [];
    private static readonly HashSet<Assembly> Shared = []; // Holds both .Shared and .Common modules
    private static readonly HashSet<Assembly> Server = [];

    private static readonly IReadOnlyList<ModuleMap> ModuleTypes = new[]
    {
        new ModuleMap(typeof(GameClient), Client),
        new ModuleMap(typeof(GameServer), Server),
        new ModuleMap(typeof(GameShared), Shared),
    };

    private static readonly Lazy<bool> Discovered = new Lazy<bool>(() =>
    {
        LoadCore();
        LoadExtras();

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!string.IsNullOrEmpty(assembly.Location))
            {
                if (!assembly.FullName!.StartsWith(ContentPrefix))
                    continue;

                AssignModule(assembly);
            }
        }

        return true;
    },
    LazyThreadSafetyMode.ExecutionAndPublication);

    private static void DiscoverModules()
    {
        _ = Discovered.Value;
    }

    /// <summary>
    /// This is required for programs that don't explicitly load core modules by themselves.
    /// For example, Content.YAMLLinter.
    /// </summary>
    private static void LoadCore()
    {
        var coreModules = Suffixes.Select(suffix => ContentPrefix + suffix[1..]).ToArray();
        LoadAssemblies(fileName => coreModules.Contains(fileName));
    }

    #region Modules
    private static void LoadExtras()
    {
        // Load modules from Modules/
        var dir = Path.GetDirectoryName(CurrentAssembly.Location);
        if (!string.IsNullOrEmpty(dir))
        {
            var modulesPath = Path.Combine(dir, "..", "..", "Modules");
            if (Directory.Exists(modulesPath))
                LoadModulesFromDirectory(modulesPath, dir);
        }
    }

    private static void LoadModulesFromDirectory(string modulesPath, string binDir)
    {
        foreach (var manifestPath in Directory.GetFiles(modulesPath, "module.yml", SearchOption.AllDirectories))
        {
            try
            {
                var manifest = Packaging.ModuleManifestLoader.LoadFromFile(manifestPath);

                foreach (var project in manifest.Projects)
                {
                    var projectName = Packaging.ModuleManifestLoader.GetProjectName(project);
                    var dllPath = Path.Combine(binDir, $"{projectName}.dll");

                    if (File.Exists(dllPath) && !AlreadyLoaded(dllPath))
                        Assembly.LoadFrom(dllPath);
                }
            }
            catch (Exception ex)
            {
                // Log warning but continue - don't break tests due to module loading issues
                Console.WriteLine($"Warning: Failed to load module from {manifestPath}: {ex.Message}");
            }
        }
    }

    #endregion

    private static void LoadAssemblies(Func<string, bool> fileFilter)
    {
        var dir = Path.GetDirectoryName(CurrentAssembly.Location);

        if (string.IsNullOrEmpty(dir))
            return;

        var dlls = Directory.GetFiles(dir, "*.dll");
        var matchingDlls = dlls.Where(file => fileFilter(Path.GetFileNameWithoutExtension(file)));

        foreach (var dll in matchingDlls)
        {
            if (!AlreadyLoaded(dll))
            {
                Assembly.LoadFrom(dll);
            }
        }
    }

    private static void AssignModule(Assembly asm)
    {
        var types = asm.GetExportedTypes();

        foreach (var type in types)
        {
            foreach (var mapping in ModuleTypes)
            {
                if (!mapping.Type.IsAssignableFrom(type))
                    continue;

                mapping.Col.Add(asm);
                return;
            }
        }
    }

    /// <summary>
    /// Retrieve content assemblies
    /// </summary>
    /// <param name="client">True to receive client assemblies, server otherwise.</param>
    /// <param name="includePoolAssembly">To include PoolManager's assembly. Required for itself, not so much for tests</param>
    /// <returns></returns>
    public static Assembly[] GetAssemblies(bool client, bool includePoolAssembly = true)
    {
        // Start with the base assemblies based on client flag
        var assemblies = new List<Assembly>(client ? Client.Concat(Shared) : Server.Concat(Shared));

        // Add pool assembly if needed
        if (includePoolAssembly)
        {
            assemblies.Add(CurrentAssembly);
        }

        return assemblies.ToArray();
    }

    private static bool AlreadyLoaded(string dll)
    {
        var assemblyName = AssemblyName.GetAssemblyName(dll);

        return AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => AssemblyName.ReferenceMatchesDefinition(
                assemblyName,
                a.GetName()));
    }
}

internal readonly record struct ModuleMap(Type Type, HashSet<Assembly> Col);
