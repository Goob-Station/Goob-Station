using Content.Servesey.Server.Harmony;
using Content.Servesey.Server.IoC;
using Robust.Shared.ContentPack;
using Robust.Shared.IoC;

namespace Content.Servesey.Server;

public sealed class EntryPoint : GameServer
{
    private IHarmonyManager _harmonyManager = default!;

    public override void PreInit()
    {
        base.PreInit();

        ServeseyContentIoC.Register();
        IoCManager.BuildGraph();

        _harmonyManager = IoCManager.Resolve<IHarmonyManager>();
        _harmonyManager.Initialize();
    }

    protected override void Dispose(bool disposing)
    {
        _harmonyManager.Shutdown();

        base.Dispose(disposing);
    }
}
