namespace Content.Casino.Server.Entry;

internal static class CasinoIoC
{
    internal static void Register()
    {
        var instance = IoCManager.Instance!;

        instance.Register<IServerCasinoManager, CasinoManager>();
    }
}
