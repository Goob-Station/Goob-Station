namespace Content.Casino.Client.Entry;

public sealed class ClientCasinoIoC
{
    public static void Register()
    {
        var instance = IoCManager.Instance!;

        instance.Register<ClientGameManager>();
    }
}
