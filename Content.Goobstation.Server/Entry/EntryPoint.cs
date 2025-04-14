using Content.Goobstation.Server.IoC;
using Content.Goobstation.Server.MisandryBox.GrassEnforce;
using Robust.Shared.ContentPack;
using Robust.Shared.IoC;

namespace Content.Goobstation.Server.Entry;

public sealed class EntryPoint : GameServer
{
    private IGrassEnforcementManager? _grass;

    public override void Init()
    {
        base.Init();

        ServerGoobContentIoC.Register();

        IoCManager.BuildGraph();

        _grass = IoCManager.Resolve<IGrassEnforcementManager>();
        _grass.Initialize();
    }

    protected override void Dispose(bool disposing)
    {
        _grass?.Shutdown();
    }
}
