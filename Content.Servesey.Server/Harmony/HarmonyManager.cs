using System.Reflection;
using HarmonyLib;
using Robust.Shared.Log;

namespace Content.Servesey.Server.Harmony;

public sealed class HarmonyManager : IHarmonyManager
{
    private const string HarmonyId = "com.servesey.engine";

    private HarmonyLib.Harmony? _harmony;
    private readonly ISawmill _sawmill = Logger.GetSawmill("servesey.harmony");
    private readonly List<PatchRecord> _appliedPatches = new();

    public void Initialize()
    {
        _sawmill.Info("Servesey initialized");

        // LocalBuild SUCKS on .NET 9 for some reason and I cant be fucked to figure why
        Environment.SetEnvironmentVariable("MONOMOD_DMD_TYPE", "dynamicmethod");

        _harmony = new HarmonyLib.Harmony(HarmonyId);

        var assembly = typeof(HarmonyManager).Assembly;
        var patchTypes = GetPatchTypes(assembly);

        _sawmill.Debug($"We have about {patchTypes.Count} patches here.");

        foreach (var patchType in patchTypes)
        {
            try
            {
                _harmony.CreateClassProcessor(patchType).Patch();

                var record = new PatchRecord(patchType.FullName ?? patchType.Name, true);
                _appliedPatches.Add(record);

                _sawmill.Debug($"Applied patch: {record.Name}");

                var initMethod = patchType.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Static, Type.EmptyTypes);
                initMethod?.Invoke(null, null);
            }
            catch (Exception ex)
            {
                var record = new PatchRecord(patchType.FullName ?? patchType.Name, false);
                _appliedPatches.Add(record);

                _sawmill.Error($"Failed to apply patch '{record.Name}': {ex}");
            }
        }

        var succeeded = _appliedPatches.Count(r => r.Success);
        var failed = _appliedPatches.Count(r => !r.Success);

        _sawmill.Info($"Servesey is done here. Applied: {succeeded}, Failed: {failed}");
    }

    public void Shutdown()
    {
        if (_harmony == null)
            return;

        _sawmill.Info("Folding patches");
        _harmony.UnpatchAll(HarmonyId);
        _appliedPatches.Clear();
        _harmony = null;
        _sawmill.Info("Unpatched everything");
    }

    private static List<Type> GetPatchTypes(Assembly assembly)
    {
        var result = new List<Type>();

        foreach (var type in assembly.GetTypes())
        {
            if (type.GetCustomAttribute<HarmonyPatch>() != null)
            {
                result.Add(type);
            }
        }

        return result;
    }

    private sealed record PatchRecord(string Name, bool Success);
}
