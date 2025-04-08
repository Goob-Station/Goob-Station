// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Robust.Shared.ContentPack;

namespace Content.IntegrationTests;

public static partial class PoolManager
{
    // Modules that match ContentPrefix+suffix[..1] are considered "core" modules.
    // So, Content.Shared, Content.Client, Content.Server are "core" modules
    // Content.Common is not a thing by default but will be considered a core module if found.
    private static readonly string ContentPrefix = "Content.";
    private static readonly string[] Suffixes = [".Shared", ".Client", ".Server", ".Common"];

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

    private static void LoadExtras()
    {
        LoadAssemblies(fileName =>
        {
            if (!fileName.StartsWith(ContentPrefix))
                return false;

            var matchingSuffix = Suffixes.FirstOrDefault(s => fileName.EndsWith(s));
            if (matchingSuffix == null)
                return false;

            // Check if module has a middle part to differentiate from core.
            var middlePartLength = fileName.Length - ContentPrefix.Length - matchingSuffix.Length;
            return middlePartLength > 0;
        });
    }

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