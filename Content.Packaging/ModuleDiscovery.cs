using AsmResolver.DotNet;
using Content.ModuleManager;

namespace Content.Packaging;

public static class ModuleDiscovery
{
    public record ModuleInfo(string Name, string ProjectPath, ModuleType Type);

    public static IEnumerable<ModuleInfo> DiscoverModules(string path = ".")
    {
        var discoveredAssemblies = new HashSet<string>();

        foreach (var dllPath in Directory.EnumerateFiles(path, "Content.*.dll", SearchOption.AllDirectories))
        {
            // Skip duplicates using assembly full name
            var assemblyName = Path.GetFileNameWithoutExtension(dllPath);
            if (!discoveredAssemblies.Add(assemblyName))
                continue;

            var module = ModuleDefinition.FromFile(dllPath);
            var assembly = module.Assembly;

            var attr = assembly?.CustomAttributes
                .FirstOrDefault(a =>
                    a.Constructor?.DeclaringType?.FullName == typeof(ContentModuleAttribute).FullName);

            if (attr?.Signature == null)
                continue;

            var dir = Path.GetDirectoryName(dllPath);
            var dirName = Path.GetFileName(dir);

            // Safely get module type with fallback
            var moduleType = attr.Signature.FixedArguments.Count > 0
                ? (ModuleType)(attr.Signature.FixedArguments[0].Element ?? 0)
                : ModuleType.Shared; // Default fallback

            if (dirName != null)
            {
                yield return new ModuleInfo(
                    assemblyName,
                    Path.Combine(dirName, $"{assemblyName}.csproj"),
                    moduleType
                );
            }
        }
    }
}
