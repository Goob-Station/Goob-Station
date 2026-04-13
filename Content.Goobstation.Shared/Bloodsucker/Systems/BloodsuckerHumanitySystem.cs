using Content.Goobstation.Shared.Bloodsuckers.Components;
using Content.Goobstation.Shared.Bloodsuckers.Events;
using Robust.Shared.GameObjects;
using System;

namespace Content.Goobstation.Shared.Bloodsuckers.Systems;

/// <summary>
/// Manages the Bloodsucker's Humanity score.
/// </summary>
public sealed class BloodsuckerHumanitySystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodsuckerHumanityComponent, ComponentInit>(OnHumanityInit);
    }

    private void OnHumanityInit(Entity<BloodsuckerHumanityComponent> ent, ref ComponentInit args)
    {
        SyncFrenzyThreshold(ent);
    }

    /// <summary>
    /// Adjusts humanity. Clamps to 0
    /// </summary>
    public void ChangeHumanity(Entity<BloodsuckerHumanityComponent> ent, float delta)
    {
        var comp = ent.Comp;
        var oldHumanity = comp.CurrentHumanity;
        comp.CurrentHumanity = Math.Clamp(comp.CurrentHumanity + delta, 0f, comp.MaxHumanity);

        if (MathF.Abs(comp.CurrentHumanity - oldHumanity) < 0.001f)
            return;

        // Recalculate frenzy threshold on parent component
        SyncFrenzyThreshold(ent);

        // Broadcast change
        var changedEv = new BloodsuckerHumanityChangedEvent(oldHumanity, comp.CurrentHumanity);
        RaiseLocalEvent(ent, ref changedEv);

        // Check gated-action threshold crossing
        var threshold = comp.GatedActionMinHumanity;
        var wasAbove = oldHumanity >= threshold;
        var isAbove = comp.CurrentHumanity >= threshold;

        if (wasAbove && !isAbove)
            RaiseLocalEvent(ent, new BloodsuckerActionsGatedEvent());
        else if (!wasAbove && isAbove)
            RaiseLocalEvent(ent, new BloodsuckerActionsRestoredEvent());

        Dirty(ent);
    }

    #region helpers
    /// <summary>
    /// Linearly interpolates the frenzy threshold between the two extremes.
    /// </summary>
    private void SyncFrenzyThreshold(Entity<BloodsuckerHumanityComponent> ent)
    {
        if (!TryComp(ent, out BloodsuckerComponent? bloodsucker))
            return;

        var humanity = ent.Comp;
        var t = humanity.MaxHumanity > 0f
            ? humanity.CurrentHumanity / humanity.MaxHumanity
            : 0f;

        // Lerp: t=1 (full humanity) → low threshold (frenzy late)
        //       t=0 (no humanity)   → high threshold (frenzy early)
        bloodsucker.FrenzyThreshold = MathHelper.Lerp(
            humanity.FrenzyThresholdAtZeroHumanity,
            humanity.FrenzyThresholdAtMaxHumanity,
            t);

        Dirty(ent.Owner, bloodsucker);
    }

    /// <summary>
    /// Computes the burn-damage-per-second that should be applied during a frenzy session, linearly scaled by current humanity.
    /// </summary>
    public float GetFrenzyBurnDamage(Entity<BloodsuckerHumanityComponent> ent)
    {
        var humanity = ent.Comp;
        var t = humanity.MaxHumanity > 0f
            ? humanity.CurrentHumanity / humanity.MaxHumanity
            : 0f;

        return MathHelper.Lerp(
            humanity.FrenzyBurnDamageAtZeroHumanity,
            humanity.FrenzyBurnDamageAtMaxHumanity,
            t);
    }

    #endregion
}
