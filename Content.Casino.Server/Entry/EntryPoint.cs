using Robust.Shared.ContentPack;

namespace Content.Casino.Server.Entry;

public sealed class EntryPoint : GameServer
{
    public override void Init()
    {
        CasinoIoC.Register();

        IoCManager.BuildGraph();
        IoCManager.InjectDependencies(this);
    }
}
