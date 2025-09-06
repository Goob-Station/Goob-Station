using Content.Goobstation.Shared.LightDetection.Components;
using Content.Shared.Alert;

namespace Content.Goobstation.Shared.LightDetection.Systems;

public abstract class SharedLightDetectionDamageSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LightDetectionDamageComponent, MapInitEvent>(OnStartup);
        SubscribeLocalEvent<LightDetectionDamageComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(EntityUid uid, LightDetectionDamageComponent component, MapInitEvent args)
    {
        if (component.ShowAlert)
            _alerts.ShowAlert(uid, component.AlertProto);

        component.DetectionValue = component.DetectionValueMax;
    }

    private void OnShutdown(EntityUid uid, LightDetectionDamageComponent component, ComponentShutdown args)
    {
        _alerts.ClearAlert(uid, component.AlertProto);
    }
}
