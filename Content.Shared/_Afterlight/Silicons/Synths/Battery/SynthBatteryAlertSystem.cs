using Content.Shared.Alert;
using Content.Shared.Mobs;
using Content.Shared.Power.EntitySystems;

namespace Content.Shared._Afterlight.Silicons.Synths.Battery;

public sealed partial class SynthBatteryAlertSystem : EntitySystem
{
    [Dependency] private AlertsSystem _alerts = default!;
    [Dependency] private SharedBatterySystem _battery = default!;
    [Dependency] private SynthBatterySystem _synthBattery = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SynthBatteryComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    public void UpdateBatteryAlert(Entity<SynthBatteryComponent> ent)
    {
        if (!_synthBattery.TryGetBattery(ent.Owner, out var battery))
        {
            _alerts.ClearAlert(ent.Owner, ent.Comp.BatteryAlert);
            _alerts.ShowAlert(ent.Owner, ent.Comp.NoBatteryAlert);
            return;
        }

        var severity = (short) MathF.Round(_battery.GetChargeLevel(battery.Value.AsNullable()) * 10f);

        if (severity == 0 && _battery.GetCharge(battery.Value.AsNullable()) > 0f)
            severity = 1;

        _alerts.ClearAlert(ent.Owner, ent.Comp.NoBatteryAlert);
        _alerts.ShowAlert(ent.Owner, ent.Comp.BatteryAlert, severity);
    }

    public void ClearBatteryAlerts(Entity<SynthBatteryComponent> ent)
    {
        _alerts.ClearAlert(ent.Owner, ent.Comp.BatteryAlert);
        _alerts.ClearAlert(ent.Owner, ent.Comp.NoBatteryAlert);
    }

    private void OnMobStateChanged(Entity<SynthBatteryComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead)
        {
            ClearBatteryAlerts(ent);
            return;
        }

        UpdateBatteryAlert(ent);
    }
}
