using Robust.Shared.IoC;

namespace Content.Goobstation.Shared.IoC;

internal static class SharedGoobContentIoC
{
    internal static void Register()
    {
        var instance = IoCManager.Instance!;
    }
}
