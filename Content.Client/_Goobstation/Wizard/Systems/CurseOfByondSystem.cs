using Content.Shared._Goobstation.Wizard.Components;
using Robust.Shared;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
namespace Content.Client._Goobstation.Wizard.Systems;

public sealed class CurseOfByondSystem : EntitySystem
{
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    public bool InitPredict;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CurseOfByondComponent, LocalPlayerAttachedEvent>(OnAttached);
        SubscribeLocalEvent<CurseOfByondComponent, LocalPlayerDetachedEvent>(OnDetached);
        SubscribeLocalEvent<CurseOfByondComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<CurseOfByondComponent, ComponentShutdown>(OnShutdown);
        InitPredict = _cfg.GetCVar(CVars.NetPredict);
    }

    private void OnStartup(EntityUid uid, CurseOfByondComponent component, ComponentStartup args)
    {
        if (uid == _player.LocalEntity)
            _cfg.SetCVar(CVars.NetPredict, false);
    }

    private void OnShutdown(EntityUid uid, CurseOfByondComponent component, ComponentShutdown args)
    {
        if (uid == _player.LocalEntity)
            _cfg.SetCVar(CVars.NetPredict, InitPredict);
    }

    private void OnDetached(EntityUid uid, CurseOfByondComponent component, LocalPlayerDetachedEvent args)
    {
        _cfg.SetCVar(CVars.NetPredict, InitPredict);
    }

    private void OnAttached(EntityUid uid, CurseOfByondComponent component, LocalPlayerAttachedEvent args)
    {
        _cfg.SetCVar(CVars.NetPredict, false);
    }
}