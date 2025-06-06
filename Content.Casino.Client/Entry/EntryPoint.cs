using Robust.Shared.ContentPack;

namespace Content.Casino.Client.Entry;

public sealed class EntryPoint : GameClient
{
    public override void Init()
    {
        base.Init();

        ClientCasinoIoC.Register();

        IoCManager.BuildGraph();
        IoCManager.InjectDependencies(this);
    }
}
