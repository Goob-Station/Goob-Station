using Content.Shared._Goobstation.Wizard.Components;
using Content.Shared.Alert;

namespace Content.Server._Goobstation.Wizard.Systems;

public sealed class CurseOfByondSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alertsSystem = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CurseOfByondComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<CurseOfByondComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(EntityUid uid, CurseOfByondComponent component, ComponentStartup args)
    {
        _alertsSystem.ShowAlert(uid, component.CurseOfByondAlertKey);
    }

    private void OnShutdown(EntityUid uid, CurseOfByondComponent component, ComponentShutdown args)
    {
        _alertsSystem.ClearAlert(uid, component.CurseOfByondAlertKey);
    }
}