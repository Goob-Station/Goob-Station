// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Damage;
using JetBrains.Annotations;

namespace Content.Shared._Lavaland.MobPhases;

public sealed class MobPhasesSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MobPhasesComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<MobPhasesComponent, DamageChangedEvent>(OnDamage);
    }

    private void OnInit(Entity<MobPhasesComponent> ent, ref MapInitEvent args)
        => ent.Comp.PhaseThresholds = ent.Comp.BasePhaseThresholds;

    private void OnDamage(Entity<MobPhasesComponent> ent, ref DamageChangedEvent args)
        => UpdatePhases(ent.Owner);

    /// <summary>
    /// Updates current phase according to its thresholds.
    /// </summary>
    [PublicAPI]
    public void UpdatePhases(Entity<MobPhasesComponent?, DamageableComponent?> ent)
    {
        if (!Resolve(ent.Owner, ref ent.Comp1, ref ent.Comp2, false))
            return;

        var ai = ent.Comp1;
        var damageable = ent.Comp2;
        foreach (var (threshold, phase) in ai.PhaseThresholds.Reverse())
        {
            if (damageable.TotalDamage < threshold)
                continue;

            if (phase < ent.Comp1.CurrentPhase
                && !ai.CanSwitchBack)
                continue;

            ent.Comp1.CurrentPhase = phase;
            break;
        }
    }

    /// <summary>
    /// Scales all phases by one modifier. Doesn't update current phase.
    /// </summary>
    [PublicAPI]
    public void ScaleAllPhaseThresholds(Entity<MobPhasesComponent?> ent, float scale)
    {
        if (!Resolve(ent.Owner, ref ent.Comp, false))
            return;

        var thresholds = new Dictionary<FixedPoint2, int>(ent.Comp.PhaseThresholds.Reverse());
        foreach (var (damageThreshold, state) in thresholds)
        {
            // State stays the same, damage threshold is scaled.
            ent.Comp.PhaseThresholds.Remove(damageThreshold);
            ent.Comp.PhaseThresholds.Add(damageThreshold * scale, state);
        }
    }

    /// <summary>
    /// Sets phase thresholds back to default that were set on MapInit. Doesn't update current phase.
    /// </summary>
    [PublicAPI]
    public void UnscaleAllPhaseThresholds(Entity<MobPhasesComponent?> ent)
    {
        if (!Resolve(ent.Owner, ref ent.Comp, false))
            return;

        ent.Comp.PhaseThresholds = ent.Comp.BasePhaseThresholds;
    }

    [PublicAPI]
    public void SetPhaseThreshold(Entity<MobPhasesComponent?> ent, FixedPoint2 damage, int phase)
    {
        if (!Resolve(ent.Owner, ref ent.Comp, false))
            return;

        var thresholds = new Dictionary<FixedPoint2, int>(ent.Comp.PhaseThresholds);
        foreach (var (damageThreshold, state) in thresholds)
        {
            if (state != phase)
                continue;
            ent.Comp.PhaseThresholds.Remove(damageThreshold);
        }
        ent.Comp.PhaseThresholds[damage] = phase;
    }
}
