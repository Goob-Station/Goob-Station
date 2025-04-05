using Robust.Shared;
using Robust.Shared.Configuration;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.Wizard.Systems;

public sealed class CurseOfByondSystem : EntitySystem
{
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    public bool InitPredict;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<Goobstation.Shared.Wizard.CurseOfByond.CurseOfByondComponent, LocalPlayerAttachedEvent>(OnAttached);
        SubscribeLocalEvent<Goobstation.Shared.Wizard.CurseOfByond.CurseOfByondComponent, LocalPlayerDetachedEvent>(OnDetached);
        SubscribeLocalEvent<Goobstation.Shared.Wizard.CurseOfByond.CurseOfByondComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<Goobstation.Shared.Wizard.CurseOfByond.CurseOfByondComponent, ComponentShutdown>(OnShutdown);
        InitPredict = _cfg.GetCVar(CVars.NetPredict);
    }

    private void OnStartup(EntityUid uid, Goobstation.Shared.Wizard.CurseOfByond.CurseOfByondComponent component, ComponentStartup args)
    {
        if (uid == _player.LocalEntity)
            _cfg.SetCVar(CVars.NetPredict, false);
    }

    private void OnShutdown(EntityUid uid, Goobstation.Shared.Wizard.CurseOfByond.CurseOfByondComponent component, ComponentShutdown args)
    {
        if (uid == _player.LocalEntity)
            _cfg.SetCVar(CVars.NetPredict, InitPredict);
    }

    private void OnDetached(EntityUid uid, Goobstation.Shared.Wizard.CurseOfByond.CurseOfByondComponent component, LocalPlayerDetachedEvent args)
    {
        _cfg.SetCVar(CVars.NetPredict, InitPredict);
    }

    private void OnAttached(EntityUid uid, Goobstation.Shared.Wizard.CurseOfByond.CurseOfByondComponent component, LocalPlayerAttachedEvent args)
    {
        _cfg.SetCVar(CVars.NetPredict, false);
    }
}