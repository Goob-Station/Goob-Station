using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Robust.Shared.ContentPack;

namespace Content.IntegrationTests;

public static partial class PoolManager
{
    private static readonly string ContentPrefix = "Content.";
    private static readonly string[] Suffixes = [".Shared", ".Client", ".Server"];

    private static readonly Assembly CurrentAssembly = typeof(PoolManager).Assembly;

    private static readonly HashSet<Assembly> Client = [];
    private static readonly HashSet<Assembly> Shared = [];
    private static readonly HashSet<Assembly> Server = [];

    private static readonly IReadOnlyList<ModuleMap> ModuleTypes = new[]
    {
        new ModuleMap(typeof(GameClient), Client),
        new ModuleMap(typeof(GameServer), Server),
        new ModuleMap(typeof(GameShared), Shared),
    };

    private static void DiscoverModules()
    {
        if (Client.Count != 0 || Shared.Count != 0 || Server.Count != 0)
            throw new InvalidOperationException("DiscoverModules ran twice!");

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
    }


    private static void LoadExtras()
    {
        var dir = Path.GetDirectoryName(CurrentAssembly.Location);

        if (string.IsNullOrEmpty(dir))
            return;

        var dlls = Directory.GetFiles(dir, "*.dll");

        var match = dlls.Where(file =>
        {
            var fileName = Path.GetFileNameWithoutExtension(file);

            if (!fileName.StartsWith(ContentPrefix))
                return false;

            var matchingSuffix = Suffixes.FirstOrDefault(s => fileName.EndsWith(s));
            if (matchingSuffix == null)
                return false;

            // Check if module has a middle part to differentiate from core.
            var middlePartLength = fileName.Length - ContentPrefix.Length - matchingSuffix.Length;
            return middlePartLength > 0;
        });

        foreach (var dll in match)
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
