using Content.Goobstation.Shared.Bloodsuckers.Components;
using Content.Goobstation.Shared.Bloodsuckers.Systems;
using Content.Shared.Alert;
using Content.Shared.Damage;
using Content.Shared.Popups;
using Robust.Server.Audio;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Bloodsuckers.Systems;

public sealed class BloodsuckerDayNightSystem : SharedBloodsuckerDayNightSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<BloodsuckerDayNightComponent>();
        while (query.MoveNext(out var uid, out var cycle))
        {
            cycle.TimeUntilCycle -= frameTime;
            if (cycle.IsDaytime)
                UpdateDay(uid, cycle, frameTime);
            else
                UpdateNight(uid, cycle);

            UpdateVampireAlerts(cycle);
            Dirty(uid, cycle);
        }
    }

    private void UpdateNight(EntityUid uid, BloodsuckerDayNightComponent cycle)
    {
        // Send warnings as thresholds are crossed
        if (!cycle.SentFirstWarning && cycle.TimeUntilCycle <= cycle.WarnFirst)
        {
            cycle.SentFirstWarning = true;
            BroadcastToVampires(Loc.GetString("bloodsucker-sol-warn-first",
                ("minutes", MathF.Round(cycle.WarnFirst / 60f))),
                PopupType.MediumCaution);
        }

        if (!cycle.SentFinalWarning && cycle.TimeUntilCycle <= cycle.WarnFinal)
        {
            cycle.SentFinalWarning = true;
            BroadcastToVampires(Loc.GetString("bloodsucker-sol-warn-final",
                ("seconds", (int) cycle.WarnFinal)),
                PopupType.LargeCaution);
        }

        if (!cycle.SentImminentWarning && cycle.TimeUntilCycle <= cycle.WarnImminent)
        {
            cycle.SentImminentWarning = true;
            BroadcastToVampires(Loc.GetString("bloodsucker-sol-warn-imminent"),
                PopupType.LargeCaution);
        }

        if (cycle.TimeUntilCycle > 0f)
            return;

        // Day begins
        cycle.IsDaytime = true;
        cycle.TimeUntilCycle = RollDayDuration(cycle);
        cycle.SentFirstWarning = false;
        cycle.SentFinalWarning = false;
        cycle.SentImminentWarning = false;

        var dayEv = new BloodsuckerDayStartedEvent();
        RaiseLocalEvent(uid, ref dayEv, broadcast: true);

        BroadcastToVampires(Loc.GetString("bloodsucker-sol-day-start",
            ("minutes", MathF.Round(cycle.TimeUntilCycle / 60f))),
            PopupType.LargeCaution);

        _audio.PlayGlobal(cycle.DayStartSound, Filter.Empty().AddPlayers(
            GetVampireSessions()), false);
    }

    private void UpdateDay(EntityUid uid, BloodsuckerDayNightComponent cycle, float frameTime)
    {
        if (cycle.TimeUntilCycle > 0f)
        {
            cycle.DayBurnAccumulator += frameTime;
            if (cycle.DayBurnAccumulator >= cycle.DayBurnTickRate)
            {
                cycle.DayBurnAccumulator -= cycle.DayBurnTickRate;
                BurnExposedVampires(cycle);
            }
            return;
        }

        // Night begins
        cycle.IsDaytime = false;
        cycle.DayBurnAccumulator = 0f;
        cycle.TimeUntilCycle = RollNightDuration(cycle);

        var burnQuery = EntityQueryEnumerator<BloodsuckerComponent>();
        while (burnQuery.MoveNext(out var vampUid, out _))
        {
            // Safe in coffin
            if (HasComp<InsideCoffinComponent>(vampUid))
                continue;

            _popup.PopupEntity(
                Loc.GetString("bloodsucker-sol-burning"),
                vampUid, vampUid, PopupType.LargeCaution);

            var burnDamage = new DamageSpecifier();
            burnDamage.DamageDict["Heat"] = cycle.DayBurnDamage;
            _damageable.TryChangeDamage(vampUid, burnDamage, ignoreResistances: true);
        }

        var nightEv = new BloodsuckerNightStartedEvent();
        RaiseLocalEvent(uid, ref nightEv, broadcast: true);

        BroadcastToVampires(Loc.GetString("bloodsucker-sol-night-start"),
            PopupType.Medium);

        _audio.PlayGlobal(cycle.DayEndSound, Filter.Empty().AddPlayers(
            GetVampireSessions()), false);
    }
    private void BurnExposedVampires(BloodsuckerDayNightComponent cycle)
    {
        var query = EntityQueryEnumerator<BloodsuckerComponent>();
        while (query.MoveNext(out var vampUid, out _))
        {
            if (HasComp<InsideCoffinComponent>(vampUid))
                continue;

            _popup.PopupEntity(
                Loc.GetString("bloodsucker-sol-burning"),
                vampUid, vampUid, PopupType.LargeCaution);

            var burn = new DamageSpecifier();
            burn.DamageDict["Heat"] = cycle.DayBurnDamage;
            _damageable.TryChangeDamage(vampUid, burn, ignoreResistances: true);
        }
    }

    private void UpdateVampireAlerts(BloodsuckerDayNightComponent cycle)
    {
        var query = EntityQueryEnumerator<BloodsuckerComponent>();
        while (query.MoveNext(out var uid, out var vamp))
        {
            // Severity drives icon state: 0=night, 1=90s, 2=60s, 3=30s, 4=day
            int severity;
            if (cycle.IsDaytime)
                severity = 4;
            else if (cycle.TimeUntilCycle <= 30f)
                severity = 3;
            else if (cycle.TimeUntilCycle <= 60f)
                severity = 2;
            else if (cycle.TimeUntilCycle <= 90f)
                severity = 1;
            else
                severity = 0;

            _alerts.ShowAlert(uid, vamp.SolAlert, (short) severity);
        }
    }

    private void BroadcastToVampires(string message, PopupType type)
    {
        var query = EntityQueryEnumerator<BloodsuckerComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            _popup.PopupEntity(message, uid, uid, type);
        }
    }

    private IEnumerable<ICommonSession> GetVampireSessions()
    {
        var sessions = new List<ICommonSession>();
        var query = EntityQueryEnumerator<BloodsuckerComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            if (TryComp(uid, out ActorComponent? actor))
                sessions.Add(actor.PlayerSession);
        }
        return sessions;
    }
}
