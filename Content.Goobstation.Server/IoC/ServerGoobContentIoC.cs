using Content.Goobstation.Server.Redial;
using Robust.Shared.IoC;

namespace Content.Goobstation.Server.IoC;

internal static class ServerGoobContentIoC
{
    internal static void Register()
    {
        var instance = IoCManager.Instance!;

        instance.Register<RedialManager>();
    }
}
