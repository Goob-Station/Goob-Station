using Content.Goobstation.Shared.Bloodsuckers.Components;
using Content.Goobstation.Shared.Bloodsuckers.Components.Actions;
using Content.Goobstation.Shared.Bloodsuckers.Events;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Popups;
using Content.Shared.StatusEffectNew;
using Content.Shared.Traits.Assorted;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;
using System.Net.NetworkInformation;

namespace Content.Goobstation.Shared.Bloodsuckers.Systems;

public abstract class SharedBloodsuckerFortitudeSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly BloodsuckerHumanitySystem _humanity = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private EntityQuery<MobStateComponent> _mobStateQuery;

    public override void Initialize()
    {
        base.Initialize();

        _mobStateQuery = GetEntityQuery<MobStateComponent>();

        SubscribeLocalEvent<BloodsuckerFortitudeComponent, BloodsuckerFortitudeEvent>(OnFortitude);
        SubscribeLocalEvent<BloodsuckerFortitudeComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnFortitude(Entity<BloodsuckerFortitudeComponent> ent, ref BloodsuckerFortitudeEvent args)
    {
        if (!HasComp<BloodsuckerComponent>(ent.Owner))
            return;

        if (ent.Comp.Active)
            Deactivate(ent);
        else
        {
            if (!TryUseCosts(ent.Owner, ent.Comp))
                return;
            Activate(ent);
        }

        args.Handled = true;
    }

    private void OnShutdown(Entity<BloodsuckerFortitudeComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.Active)
            Deactivate(ent);
    }

    protected virtual void Activate(Entity<BloodsuckerFortitudeComponent> ent)
    {
        ent.Comp.Active = true;

        // Store whatever modifier set they had before
        var levelIndex = Math.Clamp(ent.Comp.ActionLevel - 1, 0, ent.Comp.FortitudeModifierSetIds.Count - 1);
        var modifierSetId = ent.Comp.FortitudeModifierSetIds[levelIndex];

        if (TryComp(ent.Owner, out DamageableComponent? damageable))
        {
            ent.Comp.PreviousModifierSetId = damageable.DamageModifierSetId;
            _damageable.SetDamageModifierSetId(ent.Owner, modifierSetId, damageable);
        }

        // Stun immunity at level 2+
        if (ent.Comp.ActionLevel >= ent.Comp.StunImmuneLevel)
            _status.TryAddStatusEffect(ent.Owner, "StunImmunity", out _, TimeSpan.MaxValue);

        _audio.PlayPredicted(ent.Comp.ActivateSound, ent.Owner, ent.Owner);
        _popup.PopupPredicted(
            Loc.GetString("bloodsucker-fortitude-on"),
            ent.Owner, ent.Owner, PopupType.Small);

        Dirty(ent);
    }

    protected virtual void Deactivate(Entity<BloodsuckerFortitudeComponent> ent)
    {
        ent.Comp.Active = false;

        // Restore previous modifier set
        if (TryComp(ent.Owner, out DamageableComponent? damageable))
            _damageable.SetDamageModifierSetId(ent.Owner, ent.Comp.PreviousModifierSetId, damageable);

        ent.Comp.PreviousModifierSetId = null;

        _status.TryRemoveStatusEffect(ent.Owner, "StunImmunity");

        _audio.PlayPredicted(ent.Comp.DeactivateSound, ent.Owner, ent.Owner);
        _popup.PopupPredicted(
            Loc.GetString("bloodsucker-fortitude-off"),
            ent.Owner, ent.Owner, PopupType.Small);

        Dirty(ent);
    }

    protected void CheckFortitudeValidity(Entity<BloodsuckerFortitudeComponent> ent)
    {
        if (!ent.Comp.Active)
            return;

        if (_mobStateQuery.TryComp(ent.Owner, out var mobState)
            && mobState.CurrentState == MobState.Dead)
        {
            Deactivate(ent);
        }
    }
    protected void CheckRunning(Entity<BloodsuckerFortitudeComponent> ent)
    {
        if (!TryComp(ent.Owner, out InputMoverComponent? mover) || !mover.Sprinting)
            return;

        if (!TryComp(ent.Owner, out Robust.Shared.Physics.Components.PhysicsComponent? physics)
            || physics.LinearVelocity.LengthSquared() < 0.5f)
            return;

        var damage = new DamageSpecifier();
        damage.DamageDict["Blunt"] = ent.Comp.RunPenaltyDamage;
        _damageable.TryChangeDamage(ent.Owner, damage, ignoreResistances: true);

        _popup.PopupPredicted(
            Loc.GetString("bloodsucker-fortitude-run-penalty"),
            ent.Owner, ent.Owner, PopupType.SmallCaution);
    }

    private bool TryUseCosts(EntityUid uid, BloodsuckerFortitudeComponent comp)
    {
        if (comp.DisabledInFrenzy && HasComp<BloodsuckerFrenzyComponent>(uid))
            return false;

        if (comp.HumanityCost != 0f && TryComp(uid, out BloodsuckerHumanityComponent? humanity))
            _humanity.ChangeHumanity(
                new Entity<BloodsuckerHumanityComponent>(uid, humanity),
                -comp.HumanityCost);

        return true;
    }
}
