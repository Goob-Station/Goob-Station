using Content.Goobstation.Client.Redial;
using Robust.Shared.IoC;

namespace Content.Goobstation.Client.IoC;

internal static class ContentGoobClientIoC
{
    internal static void Register()
    {
        var collection = IoCManager.Instance!;

        collection.Register<RedialManager>();
    }
}
