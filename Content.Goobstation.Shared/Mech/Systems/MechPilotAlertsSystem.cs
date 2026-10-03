using Content.Goobstation.Shared.Mech.Components;
using Content.Shared.Alert;
using Content.Shared.Mech.Components;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Mech.Systems;

public sealed class MechPilotAlertsSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<MechPilotAlertsComponent, MechComponent>();
        while (query.MoveNext(out var uid, out var alerts, out var mech))
        {
            if (curTime < alerts.NextUpdate)
                continue;

            alerts.NextUpdate = curTime + alerts.UpdateInterval;
            Dirty(uid, alerts);

            var pilot = mech.PilotSlot.ContainedEntity;

            if (pilot != alerts.LastPilot && alerts.LastPilot is { } previous && Exists(previous))
                ClearAll(previous, alerts);

            if (alerts.LastPilot != pilot)
            {
                alerts.LastPilot = pilot;
                Dirty(uid, alerts);
            }

            if (pilot == null)
                continue;

            var powerSeverity = mech.MaxEnergy > 0
                ? (short) Math.Clamp(Math.Round((mech.Energy / mech.MaxEnergy).Float() * 10), 0, 10)
                : (short) 0;
            _alerts.ShowAlert(pilot.Value, alerts.PowerAlert, powerSeverity);

            var integrity = mech.MaxIntegrity > 0 ? (mech.Integrity / mech.MaxIntegrity).Float() : 1f;
            var integritySeverity = integrity < alerts.IntegrityCriticalThreshold ? (short) 3
                : integrity < alerts.IntegrityDangerThreshold ? (short) 2
                : integrity < alerts.IntegrityWarnThreshold ? (short) 1
                : (short) 0;
            _alerts.ShowAlert(pilot.Value, alerts.IntegrityAlert, integritySeverity);

            var charging = mech.OnChargingStation && mech.Energy < mech.MaxEnergy;
            if (charging)
                _alerts.ShowAlert(pilot.Value, alerts.ChargingAlert);
            else
                _alerts.ClearAlert(pilot.Value, alerts.ChargingAlert);
        }
    }

    private void ClearAll(EntityUid pilot, MechPilotAlertsComponent alerts)
    {
        _alerts.ClearAlert(pilot, alerts.PowerAlert);
        _alerts.ClearAlert(pilot, alerts.IntegrityAlert);
        _alerts.ClearAlert(pilot, alerts.ChargingAlert);
    }
}
