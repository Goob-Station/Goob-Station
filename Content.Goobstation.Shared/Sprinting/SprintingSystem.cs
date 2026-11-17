// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage.Events;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Gravity;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Standing;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;
using Robust.Shared.Physics.Events;
using Content.Goobstation.Shared.Sandevistan;

namespace Content.Goobstation.Shared.Sprinting;

public sealed partial class SprintingSystem : EntitySystem
{
    [Dependency] private SharedStaminaSystem _staminaSystem = default!;
    [Dependency] private MovementSpeedModifierSystem _movementSpeed = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedGravitySystem _gravity = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private StandingStateSystem _standing = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private SharedMoverController _moverController = default!;
    [Dependency] private SharedStunSystem _stunSystem = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // We dont add it to the EQE since the comp might get added as this runs.
        var query = EntityQueryEnumerator<SprinterComponent, StaminaModifierStatusEffectComponent>();
        while (query.MoveNext(out var uid, out var sprinterComp, out var staminaComp))
        {
            if (!sprinterComp.IsSprinting
                || !sprinterComp.ScaleWithStamina
                || staminaComp.Modifier <= 1f)
                continue;

            _staminaSystem.ModifyStaminaDrain(uid,
                sprinterComp.StaminaDrainKey,
                sprinterComp.StaminaDrainRate * staminaComp.Modifier * sprinterComp.StaminaDrainMultiplier);
        }
    }

    public void SetSprinting(
        Entity<SprinterComponent?> ent,
        bool newState,
        bool gracefulStop = true,
        TimeSpan? cooldownOverride = null)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        if (ent.Comp.IsSprinting == newState)
            return;

        if (_timing.CurTime < ent.Comp.TimeBetweenSprints)
            return;

        if (newState && !CanSprint(ent, out var failLoc))
        {
            _popup.PopupEntity(Loc.GetString(failLoc), ent, ent, PopupType.Medium);
            return;
        }

        ent.Comp.NextSprintAllowed = _timing.CurTime + (cooldownOverride ?? ent.Comp.TimeBetweenSprints);
        ent.Comp.IsSprinting = newState;
        Dirty(ent);

        if (newState)
        {
            RaiseLocalEvent(ent, new SprintStartEvent());
            _audio.PlayPredicted(ent.Comp.SprintStartupSound, ent, ent);
        }

        if (!gracefulStop)
            _damageable.TryChangeDamage(ent.Owner, ent.Comp.SprintDamageSpecifier);

        _movementSpeed.RefreshMovementSpeedModifiers(ent);
        _staminaSystem.ToggleStaminaDrain(
            target: ent,
            drainRate: ent.Comp.StaminaDrainRate,
            enabled: newState,
            modifiesSpeed: true,
            key: ent.Comp.StaminaDrainKey,
            source: ent);
    }

    [SubscribeLocalEvent]
    private void OnRefreshSpeed(Entity<SprinterComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (!ent.Comp.IsSprinting)
            return;

        args.ModifySpeed(ent.Comp.SprintSpeedMultiplier);
    }


    [SubscribeLocalEvent]
    private void OnSprintToggle(Entity<SprinterComponent> ent, ref SprintToggleEvent args)
    {
        SetSprinting(ent.AsNullable(), args.IsSprinting);
    }

    [SubscribeLocalEvent]
    private void OnBeforeStaminaDamage(Entity<SprinterComponent> ent, ref BeforeStaminaDamageEvent args)
    {
        if (!ent.Comp.IsSprinting
            || args.Value > 0)
            return;

        args.Value *= ent.Comp.StaminaRegenMultiplier;
    }

    [SubscribeLocalEvent]
    private void OnCollide(Entity<SprinterComponent> ent, ref StartCollideEvent args)
    {
        var ourUid = args.OurEntity;
        var otherUid = args.OtherEntity;

        if (ourUid.Id < otherUid.Id)
            return;

        if (!ent.Comp.IsSprinting)
            return;

        if (!TryComp(otherUid, out SprinterComponent? otherSprinter)
            || !otherSprinter.IsSprinting
            || !HasComp<ActiveSandevistanUserComponent>(otherUid))
        {
            return;
        }

        _stunSystem.TryKnockdown(ourUid, ent.Comp.KnockdownDurationOnInterrupt, false, true);
        _stunSystem.TryKnockdown(otherUid,
            otherSprinter.KnockdownDurationOnInterrupt,
            false,
            true);
    }
}
