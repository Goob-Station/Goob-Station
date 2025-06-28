// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.CombatMode;
using Content.Shared.GameTicking;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Players.PlayTimeTracking;
using Content.Shared.Roles.Jobs;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.MisandryBox.PlaytimeDegrade;

public sealed class PlaytimeDegradeSystem : EntitySystem
{
    [Dependency] private readonly ISharedPlaytimeManager _playtime = default!;
    [Dependency] private readonly MobThresholdSystem _mob = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;
    [Dependency] private readonly SharedJobSystem _job = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PlaytimeDegradeComponent, PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
        SubscribeLocalEvent<PlaytimeDegradeComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovementSpeed);
    }

    private void OnPlayerSpawnComplete(Entity<PlaytimeDegradeComponent> ent, ref PlayerSpawnCompleteEvent args)
    {
        if (!TryGetDecayRatio(ent, args.Player, out var decayRatio, out var effectiveMinutes))
            return;

        ent.Comp.DecayRatio = decayRatio;
        Degrade(ent, decayRatio.Value, effectiveMinutes.Value);
        ApplyDisarmMalus(ent, decayRatio.Value);
        _movement.RefreshMovementSpeedModifiers(ent.Owner);
    }

    private void OnRefreshMovementSpeed(Entity<PlaytimeDegradeComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.DecayRatio.HasValue)
        {
            var speedRatio = 1.0f - (ent.Comp.DecayRatio.Value * (1.0f - ent.Comp.Floor));
            args.ModifySpeed(speedRatio);
        }
    }

    private bool TryGetDecayRatio(Entity<PlaytimeDegradeComponent> ent,
        ICommonSession player,
        [NotNullWhen(true)] out float? decayRatio,
        [NotNullWhen(true)] out int? effectiveMinutes)
    {
        decayRatio = null;
        effectiveMinutes = null;

        DebugTools.Assert(ent.Comp.Until >= ent.Comp.Since, "PlaytimeDegrade Until must be greater than Since");
        if (ent.Comp.Until <= ent.Comp.Since)
            return false;

        if (!_mind.TryGetMind(ent.Owner, out var mind, out var _) ||
            !_job.MindTryGetJob(mind, out var jobPrototype))
            return false;

        var playtimes = _playtime.GetPlayTimes(player);
        var jobPlaytime = playtimes.FirstOrNull(e => e.Key == jobPrototype.PlayTimeTracker);

        if (jobPlaytime is null)
            return false;

        var totalMinutes = (int)jobPlaytime.Value.Value.TotalMinutes;

        if (totalMinutes < ent.Comp.Since)
            return false;

        effectiveMinutes = Math.Min(totalMinutes, ent.Comp.Until) - ent.Comp.Since;
        var totalDecayMinutes = ent.Comp.Until - ent.Comp.Since;
        decayRatio = (float)effectiveMinutes / totalDecayMinutes;

        return true;
    }

    private void Degrade(Entity<PlaytimeDegradeComponent> ent, float decayRatio, int effectiveMinutes)
    {
        if (!_mob.TryGetThresholdForState(ent.Owner, MobState.Critical, out var criticalThreshold) ||
            !_mob.TryGetThresholdForState(ent.Owner, MobState.Dead, out var deadThreshold))
            return;

        var critFloor = criticalThreshold.Value * ent.Comp.Floor;
        var deadFloor = deadThreshold.Value * ent.Comp.Floor;

        FixedPoint2 newCritThreshold, newDeadThreshold;

        if (ent.Comp.By.HasValue)
        {
            var totalDecay = ent.Comp.By.Value * effectiveMinutes;
            newCritThreshold = FixedPoint2.Max(critFloor, criticalThreshold.Value - totalDecay);
            newDeadThreshold = FixedPoint2.Max(deadFloor, deadThreshold.Value - totalDecay);
        }
        else
        {
            var critDecay = (criticalThreshold.Value - critFloor) * decayRatio;
            var deadDecay = (deadThreshold.Value - deadFloor) * decayRatio;

            newCritThreshold = criticalThreshold.Value - critDecay;
            newDeadThreshold = deadThreshold.Value - deadDecay;
        }

        _mob.SetMobStateThreshold(ent.Owner, newCritThreshold, MobState.Critical);
        _mob.SetMobStateThreshold(ent.Owner, newDeadThreshold, MobState.Dead);
    }

    private void ApplyDisarmMalus(Entity<PlaytimeDegradeComponent> ent, float decayRatio)
    {
        if (!ent.Comp.DisarmMalus || !(decayRatio >= 0.5f))
            return;

        EnsureComp<DisarmMalusComponent>(ent.Owner, out var malus);
        malus.Malus = decayRatio;
    }
}
