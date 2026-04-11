using Content.Goobstation.Shared.Bloodsuckers.Components;
using Content.Goobstation.Shared.Bloodsuckers.Events;
using Content.Shared.Bed.Sleep;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Flash;
using Content.Shared.Interaction;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;

namespace Content.Goobstation.Shared.Bloodsuckers.Systems;

/// <summary>
/// Handles all passive effects of the Bloodsucker antagonist.
/// </summary>
public sealed class BloodsuckerSystem : EntitySystem
{
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodsuckerComponent, FlashDurationMultiplierEvent>(OnFlashAttempt);
        SubscribeLocalEvent<BloodsuckerComponent, DamageModifyEvent>(OnDamageModify);
        SubscribeLocalEvent<BloodsuckerComponent, SleepStateChangedEvent>(OnSleepStateChanged);
        SubscribeLocalEvent<BloodsuckerComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<BloodsuckerComponent, ExaminedEvent>(OnExamined);
    }

    private void OnFlashAttempt(Entity<BloodsuckerComponent> ent, ref FlashDurationMultiplierEvent args)
    {
        var ev = new BloodsuckerFlashedEvent(1f);
        RaiseLocalEvent(ent, ref ev);

        args.Multiplier *= ev.ModifiedDuration * ent.Comp.FlashDurationMultiplier;
    }

    private void OnDamageModify(Entity<BloodsuckerComponent> ent, ref DamageModifyEvent args)
    {
        // Skip immunities while masquerading
        if (ent.Comp.IsMasquerading)
            return;

        // Negative damage = healing
        if (args.Damage.DamageDict.TryGetValue("Burn", out var burn) && burn < 0)
        {
            var cancel = new BloodsuckerBurnHealAttemptEvent();
            RaiseLocalEvent(ent, ref cancel);

            if (cancel.Cancelled)
            {
                args.Damage.DamageDict["Burn"] = 0;
                return;
            }

            if (ent.Comp.ClaimedCoffin is not { } coffin || !EntityManager.EntityExists(coffin))
            {
                args.Damage.DamageDict["Burn"] = 0;
                return;
            }

            if (!IsNearCoffin(ent, coffin))
            {
                args.Damage.DamageDict["Burn"] = 0;
                return;
            }
        }

        if (args.Damage.DamageDict.TryGetValue("Toxin", out _))
            args.Damage.DamageDict["Toxin"] = 0;

        if (args.Damage.DamageDict.TryGetValue("Asphyxiation", out _))
            args.Damage.DamageDict["Asphyxiation"] = 0;
    }

    private void OnSleepStateChanged(Entity<BloodsuckerComponent> ent, ref SleepStateChangedEvent args)
    {
        if (!args.FellAsleep)
            return;

        // Immediately wake the vampire back up because they aren't supposed to sleep
        _statusEffects.TryRemoveStatusEffect(ent, "ForcedSleep");
        _statusEffects.TryRemoveStatusEffect(ent, "Unconscious");
    }

    private void OnMobStateChanged(Entity<BloodsuckerComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Critical)
            return;

        if (ent.Comp.IsMasquerading)
            return;

        // Force the entity back to alive — bloodsuckers skip crit entirely
        _mobState.ChangeMobState(ent, MobState.Alive);
    }

    private void OnExamined(Entity<BloodsuckerComponent> ent, ref ExaminedEvent args)
    {
        if (ent.Comp.IsMasquerading)
            return;

        args.PushMarkup(
            $"[color=lightblue]{Loc.GetString("bloodsucker-examined-pale", ("target", ent.Owner))}[/color]");
    }

    #region helpers

    /// <summary>
    /// Returns true if within one tile of coffin.
    /// </summary>
    private bool IsNearCoffin(EntityUid bloodsucker, EntityUid coffin)
    {
        if (!TryComp(bloodsucker, out TransformComponent? suckerXform) ||
            !TryComp(coffin, out TransformComponent? coffinXform))
            return false;

        if (suckerXform.MapID != coffinXform.MapID)
            return false;

        var delta = suckerXform.WorldPosition - coffinXform.WorldPosition;
        return MathF.Abs(delta.X) <= 1.5f && MathF.Abs(delta.Y) <= 1.5f;
    }

    #endregion
}
