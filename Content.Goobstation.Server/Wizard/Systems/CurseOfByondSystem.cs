using Content.Shared.Alert;

namespace Content.Goobstation.Server.Wizard.Systems;

public sealed class CurseOfByondSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alertsSystem = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<Goobstation.Shared.Wizard.CurseOfByond.CurseOfByondComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<Goobstation.Shared.Wizard.CurseOfByond.CurseOfByondComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(EntityUid uid, Goobstation.Shared.Wizard.CurseOfByond.CurseOfByondComponent component, ComponentStartup args)
    {
        _alertsSystem.ShowAlert(uid, component.CurseOfByondAlertKey);
    }

    private void OnShutdown(EntityUid uid, Goobstation.Shared.Wizard.CurseOfByond.CurseOfByondComponent component, ComponentShutdown args)
    {
        _alertsSystem.ClearAlert(uid, component.CurseOfByondAlertKey);
    }
}