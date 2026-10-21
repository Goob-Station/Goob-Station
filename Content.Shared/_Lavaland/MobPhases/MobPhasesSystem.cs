// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Reflection.PortableExecutable;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Lavaland.Megafauna.Events;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using JetBrains.Annotations;
using Robust.Shared.Timing;

namespace Content.Shared._Lavaland.MobPhases;

/// <summary>
/// Moves bosses between phases.
/// A phase is entered by reaching a damage threshold or by dying a set number of times, see <see cref="MobPhase"/>.
/// Entering a phase can add and remove components on the boss. The originals are stashed and come back when the phase ends.
/// </summary>
public abstract partial class MobPhasesSystem : EntitySystem
{
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private MobThresholdSystem _mobThreshold = default!;
    [Dependency] private IGameTiming _timing = default!;

    #region Events

    [SubscribeLocalEvent]
    private void OnMapInit(Entity<MobPhasesComponent> ent, ref MapInitEvent args)
    {
        ValidatePhases(ent);
        UnscaleAllPhaseThresholds((ent.Owner, ent.Comp));

        TryGetPhase(ent.Comp, ent.Comp.CurrentPhase, out var start);
        SwapComponents(ent, ent.Comp.Stashed, null, null, start?.AddComp, start?.RemoveComp);
        ent.Comp.LastPhase = ent.Comp.CurrentPhase;
    }

    [SubscribeLocalEvent(before: [typeof(MobThresholdSystem)])]
    private void OnDamage(Entity<MobPhasesComponent> ent, ref DamageChangedEvent args)
        => UpdatePhases((ent.Owner, ent.Comp), args.Damageable);

    [SubscribeLocalEvent(after: [typeof(MobThresholdSystem)])]
    private void OnUpdateMobState(Entity<MobPhasesComponent> ent, ref UpdateMobStateEvent args)
    {
        if (args.State == MobState.Dead && TryRevive(ent))
            args.State = MobState.Alive;
    }

    [SubscribeLocalEvent]
    private void OnHandleState(Entity<MobPhasesComponent> ent, ref AfterAutoHandleStateEvent args)
        => ApplyPhaseChange(ent);

    [SubscribeLocalEvent]
    private void OnMegafaunaShutdown(Entity<MobPhasesComponent> ent, ref MegafaunaShutdownEvent args)
    {
        if (!ent.Comp.ResetOnShutdown)
            return;

        ent.Comp.Deaths = 0;
        Dirty(ent);
        SetPhase((ent.Owner, ent.Comp), 0);
    }

    #endregion

    private void ValidatePhases(Entity<MobPhasesComponent> ent)
    {
        var numbers = new HashSet<int>();
        var damages = new HashSet<FixedPoint2>();
        var deaths = new HashSet<int>();

        foreach (var phase in ent.Comp.Phases)
        {
            var name = $"Phase {phase.Phase} on {ToPrettyString(ent)}";

            if (phase.Phase < 0)
                Log.Error($"{name} has a negative number.");

            if (!numbers.Add(phase.Phase))
                Log.Error($"{name} is defined more than once.");

            if (phase.Phase != 0 && phase.Damage == null && phase.OnDeath == null)
                Log.Error($"{name} has no trigger. Set damage or onDeath.");

            if (phase.Damage is { } damage && !damages.Add(damage))
                Log.Error($"{name} shares its damage threshold with another phase.");

            if (phase.OnDeath is { } death && !deaths.Add(death))
                Log.Error($"{name} shares its onDeath with another phase.");

            if (phase.OnDeath != null && phase.HealPercentageOnDeath is <= 0f or > 1f)
                Log.Error($"{name} has a healPercentageOnDeath outside 0 to 1.");
        }
    }

    private void ApplyPhaseChange(Entity<MobPhasesComponent> ent)
    {
        if (_timing.InPrediction)
            return;

        var oldPhase = ent.Comp.LastPhase;
        var newPhase = ent.Comp.CurrentPhase;
        if (oldPhase == newPhase)
            return;

        ent.Comp.LastPhase = newPhase;

        TryGetPhase(ent.Comp, oldPhase, out var old);
        TryGetPhase(ent.Comp, newPhase, out var next);
        SwapComponents(ent, ent.Comp.Stashed, old?.AddComp, old?.RemoveComp, next?.AddComp, next?.RemoveComp);

        var ev = new MobPhaseChangedEvent(oldPhase, newPhase);
        RaiseLocalEvent(ent, ref ev);
    }

    private bool TryRevive(Entity<MobPhasesComponent> ent)
    {
        var death = ent.Comp.Deaths + 1;
        if (!TryGetRevivePhase(ent.Comp, death, out var revive)
            || !TryComp<DamageableComponent>(ent, out var damageable)
            || !_mobThreshold.TryGetDeadThreshold(ent, out var dead))
            return false;

        ent.Comp.Deaths = death;
        Dirty(ent);

        var remaining = dead.Value * (1f - revive.HealPercentageOnDeath);
        var scale = (remaining / damageable.Damage.GetTotal()).Float();
        _damageable.SetDamage((ent.Owner, damageable), damageable.Damage * scale);
        SetPhase((ent.Owner, ent.Comp), revive.Phase);
        return true;
    }

    #region API

    public static bool TryGetPhase(MobPhasesComponent comp, int number, out MobPhase phase)
    {
        foreach (var candidate in comp.Phases)
        {
            if (candidate.Phase != number)
                continue;

            phase = candidate;
            return true;
        }

        phase = null!;
        return false;
    }

    public static bool TryGetRevivePhase(MobPhasesComponent comp, int death, out MobPhase phase)
    {
        foreach (var candidate in comp.Phases)
        {
            if (candidate.OnDeath != death)
                continue;

            phase = candidate;
            return true;
        }

        phase = null!;
        return false;
    }

    /// <summary>
    /// The lowest phase the boss can go into.
    /// Without this floor <see cref="MobPhasesComponent.CanSwitchBack"/>
    /// would drop it back down multiple phases undoing the revive.
    /// </summary>
    public static int RevivePhaseFloor(MobPhasesComponent comp)
    {
        return comp.Deaths > 0 && TryGetRevivePhase(comp, comp.Deaths, out var revive)
            ? revive.Phase
            : 0;
    }

    /// <summary>
    /// Updates current phase according to its thresholds.
    /// </summary>
    [PublicAPI]
    public void UpdatePhases(Entity<MobPhasesComponent?> ent, DamageableComponent? damageable = null)
    {
        if (!Resolve(ent.Owner, ref ent.Comp, false) || !Resolve(ent.Owner, ref damageable, false))
            return;

        var phases = ent.Comp;
        var oldPhase = phases.CurrentPhase;
        var newPhase = phases.CanSwitchBack ? RevivePhaseFloor(phases) : oldPhase;

        foreach (var (threshold, phase) in phases.PhaseThresholds)
            if (damageable.TotalDamage >= threshold && phase > newPhase)
                newPhase = phase;

        if (oldPhase == newPhase)
            return;

        phases.CurrentPhase = newPhase;
        Dirty(ent.Owner, phases);
        ApplyPhaseChange((ent.Owner, phases));
    }

    /// <summary>
    /// Forces a phase regardless of damage thresholds.
    /// </summary>
    [PublicAPI]
    public void SetPhase(Entity<MobPhasesComponent?> ent, int phase)
    {
        if (!Resolve(ent.Owner, ref ent.Comp, false) || ent.Comp.CurrentPhase == phase)
            return;

        ent.Comp.CurrentPhase = phase;
        Dirty(ent);
        ApplyPhaseChange((ent.Owner, ent.Comp));
    }

    /// <summary>
    /// Scales all phases by one modifier. Doesn't update current phase.
    /// </summary>
    [PublicAPI]
    public void ScaleAllPhaseThresholds(Entity<MobPhasesComponent?> ent, float scale)
    {
        if (!Resolve(ent.Owner, ref ent.Comp, false))
            return;

        var scaled = new Dictionary<FixedPoint2, int>(ent.Comp.PhaseThresholds.Count);
        foreach (var (damage, phase) in ent.Comp.PhaseThresholds)
            scaled[damage * scale] = phase;

        ent.Comp.PhaseThresholds = scaled;
        Dirty(ent);
    }

    /// <summary>
    /// Sets phase thresholds back to default that were set on MapInit. Doesn't update current phase.
    /// </summary>
    [PublicAPI]
    public void UnscaleAllPhaseThresholds(Entity<MobPhasesComponent?> ent)
    {
        if (!Resolve(ent.Owner, ref ent.Comp, false))
            return;

        ent.Comp.PhaseThresholds.Clear();
        foreach (var phase in ent.Comp.Phases)
            if (phase.Damage is { } damage)
                ent.Comp.PhaseThresholds[damage] = phase.Phase;

        Dirty(ent);
    }

    [PublicAPI]
    public void SetPhaseThreshold(Entity<MobPhasesComponent?> ent, FixedPoint2 damage, int phase)
    {
        if (!Resolve(ent.Owner, ref ent.Comp, false))
            return;

        var thresholds = new Dictionary<FixedPoint2, int>(ent.Comp.PhaseThresholds.Count + 1);
        foreach (var (existingDamage, existingPhase) in ent.Comp.PhaseThresholds)
            if (existingPhase != phase)
                thresholds[existingDamage] = existingPhase;

        thresholds[damage] = phase;
        ent.Comp.PhaseThresholds = thresholds;
        Dirty(ent);
    }

    #endregion
}
