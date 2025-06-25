using System.Linq;
using Content.Goobstation.Maths.FixedPoint;
using Content.Server._Lavaland.Megafauna.Components;
using Content.Shared.Damage;
using JetBrains.Annotations;

namespace Content.Server._Lavaland.Megafauna.Systems;

public sealed partial class MegafaunaSystem
{
    private void InitializePhases()
    {
        SubscribeLocalEvent<PhasesMegafaunaAiComponent, ComponentStartup>(OnInit);
    }

    private void OnInit(Entity<PhasesMegafaunaAiComponent> ent, ref ComponentStartup args)
        => ent.Comp.PhaseThresholds = ent.Comp.BasePhaseThresholds;

    private void UpdatePhases(float frameTime)
    {
        var query = EntityQueryEnumerator<PhasesMegafaunaAiComponent, DamageableComponent>();
        while (query.MoveNext(out var uid, out var ai, out var damage))
        {
            ai.UpdateAccumulator -= frameTime;
            if (ai.UpdateAccumulator > 0f)
                continue;

            UpdatePhases((uid, ai, damage));
            ai.UpdateAccumulator = ai.UpdatePeriod;
        }
    }

    #region Public API

    /// <summary>
    /// Updates current phase according to its thresholds.
    /// </summary>
    [PublicAPI]
    public void UpdatePhases(Entity<PhasesMegafaunaAiComponent?, DamageableComponent?> ent)
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
    private void ScaleAllPhaseThresholds(Entity<PhasesMegafaunaAiComponent> ent, float scale)
    {
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
    private void UnscaleAllPhaseThresholds(Entity<PhasesMegafaunaAiComponent> ent)
    {
        ent.Comp.PhaseThresholds = ent.Comp.BasePhaseThresholds;
    }

    [PublicAPI]
    private void SetPhaseThreshold(Entity<PhasesMegafaunaAiComponent> ent, FixedPoint2 damage, int phase)
    {
        var thresholds = new Dictionary<FixedPoint2, int>(ent.Comp.PhaseThresholds);
        foreach (var (damageThreshold, state) in thresholds)
        {
            if (state != phase)
                continue;
            ent.Comp.PhaseThresholds.Remove(damageThreshold);
        }
        ent.Comp.PhaseThresholds[damage] = phase;
    }

    #endregion
}
