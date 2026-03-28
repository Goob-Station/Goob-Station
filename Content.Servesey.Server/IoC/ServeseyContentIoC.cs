using Content.Servesey.Server.Harmony;
using Robust.Shared.IoC;

namespace Content.Servesey.Server.IoC;

internal static class ServeseyContentIoC
{
    internal static void Register()
    {
        var instance = IoCManager.Instance!;

        instance.Register<IHarmonyManager, HarmonyManager>();
    }
}
