using Content.Goobstation.Shared.Bloodsuckers.Components;
using Content.Goobstation.Shared.Bloodsuckers.Systems;
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

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<BloodsuckerDayNightComponent>();
        while (query.MoveNext(out var uid, out var cycle))
        {
            cycle.TimeUntilCycle -= frameTime;

            if (cycle.IsDaytime)
                UpdateDay(uid, cycle);
            else
                UpdateNight(uid, cycle);

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
                ("minutes", MathF.Round(cycle.WarnFirst / 60f, 1))),
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
            ("minutes", MathF.Round(cycle.TimeUntilCycle / 60f, 1))),
            PopupType.LargeCaution);

        _audio.PlayGlobal(cycle.DayStartSound, Filter.Empty().AddPlayers(
            GetVampireSessions()), false);
    }

    private void UpdateDay(EntityUid uid, BloodsuckerDayNightComponent cycle)
    {
        if (cycle.TimeUntilCycle > 0f)
            return;

        // Night begins
        cycle.IsDaytime = false;
        cycle.TimeUntilCycle = RollNightDuration(cycle);

        var nightEv = new BloodsuckerNightStartedEvent();
        RaiseLocalEvent(uid, ref nightEv, broadcast: true);

        BroadcastToVampires(Loc.GetString("bloodsucker-sol-night-start"),
            PopupType.Medium);

        _audio.PlayGlobal(cycle.DayEndSound, Filter.Empty().AddPlayers(
            GetVampireSessions()), false);
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
