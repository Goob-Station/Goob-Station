using Content.Goobstation.Common.Weapons.DelayedKnockdown;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Cuffs.Components;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Events;
using Content.Shared.DoAfter;
using Content.Shared.Ensnaring;
using Content.Shared.Ensnaring.Components;
using Content.Shared.Hands.Components;
using Content.Shared.Heretic;
using Content.Shared.Projectiles;
using Content.Shared.StatusEffect;

namespace Content.Shared._Shitcode.Heretic.Systems.Abilities;

public abstract partial class SharedHereticAbilitySystem
{
    protected virtual void SubscribeSide()
    {
        SubscribeLocalEvent<HereticComponent, EventHereticRustCharge>(OnRustCharge);
        SubscribeLocalEvent<HereticComponent, EventHereticIceSpear>(OnIceSpear);
        SubscribeLocalEvent<HereticComponent, EventHereticRealignment>(OnRealignment);

        SubscribeLocalEvent<RealignmentComponent, StatusEffectEndedEvent>(OnStatusEnded);
        SubscribeLocalEvent<RealignmentComponent, BeforeStaminaDamageEvent>(OnBeforeRealignmentStamina);
        SubscribeLocalEvent<RealignmentComponent, ComponentShutdown>(OnRealignmentRemove);
    }

    private void OnStatusEnded(Entity<RealignmentComponent> ent, ref StatusEffectEndedEvent args)
    {
        if (args.Key != "Pacified")
            return;

        if (!Status.TryRemoveStatusEffect(ent, "Realignment"))
            RemCompDeferred(ent.Owner, ent.Comp);
    }

    private void OnRealignment(Entity<HereticComponent> ent, ref EventHereticRealignment args)
    {
        if (!TryUseAbility(ent, args))
            return;

        args.Handled = true;

        Status.TryRemoveStatusEffect(ent, "Stun");
        Status.TryRemoveStatusEffect(ent, "KnockedDown");
        Status.TryRemoveStatusEffect(ent, "ForcedSleep");
        Status.TryRemoveStatusEffect(ent, "Drowsiness");

        if (TryComp<StaminaComponent>(ent, out var stam))
        {
            if (stam.StaminaDamage >= stam.CritThreshold)
                _stam.ExitStamCrit(ent, stam);

            _stam.ToggleStaminaDrain(ent, args.StaminaRegenRate, true, true, args.StaminaRegenKey, ent);
            Dirty(ent, stam);
        }

        _standing.Stand(ent);
        RemCompDeferred<DelayedKnockdownComponent>(ent);
        _pulling.StopAllPulls(ent, stopPuller: false);
        if (Status.TryAddStatusEffect<PacifiedComponent>(ent, "Pacified", TimeSpan.FromSeconds(10f), true))
            Status.TryAddStatusEffect<RealignmentComponent>(ent, "Realignment", TimeSpan.FromSeconds(10f), true);

        // Copied shitcode below from changeling system

        if (TryComp<CuffableComponent>(ent, out var cuffs) && cuffs.Container.ContainedEntities.Count > 0)
        {
            var cuff = cuffs.LastAddedCuffs;
            _cuffs.Uncuff(ent, cuffs.LastAddedCuffs, cuff);
        }

        if (TryComp<EnsnareableComponent>(ent, out var ensnareable) && ensnareable.Container.ContainedEntities.Count > 0)
        {
            var bola = ensnareable.Container.ContainedEntities[0];
            // Yes this is dumb, but trust me this is the best way to do this. Bola code is fucking awful.
            DoAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ent, 0, new EnsnareableDoAfterEvent(), ent, ent, bola));
        }
    }


    private void OnRealignmentRemove(Entity<RealignmentComponent> ent, ref ComponentShutdown args)
    {
        _stam.ToggleStaminaDrain(ent, 0, false, true, ent.Comp.StaminaRegenKey);
    }

    private void OnBeforeRealignmentStamina(Entity<RealignmentComponent> ent, ref BeforeStaminaDamageEvent args)
    {
        if (args.Value <= 0 || args.Source == ent)
            return;

        args.Cancelled = true;
    }

    private void OnIceSpear(Entity<HereticComponent> ent, ref EventHereticIceSpear args)
    {
        if (!TryComp(args.Action, out IceSpearActionComponent? spearAction))
            return;

        if (!TryComp(ent, out HandsComponent? hands))
            return;

        if (!TryUseAbility(ent, args))
            return;

        args.Handled = true;

        if (_net.IsClient)
            return;

        if (Exists(spearAction.CreatedSpear))
        {
            var spear = spearAction.CreatedSpear.Value;

            // TODO: When heretic spells are made the way wizard spell works don't handle this action if we can't pick it up.
            // It is handled now because it always speaks invocation no matter what.
            if (_hands.IsHolding((ent.Owner, hands), spear) || !_hands.TryGetEmptyHand((ent, hands), out var hand))
                return;

            if (TryComp(spear, out EmbeddableProjectileComponent? embeddable) && embeddable.EmbeddedIntoUid != null)
                _projectile.EmbedDetach(spear, embeddable);

            _transform.AttachToGridOrMap(spear);
            _transform.SetCoordinates(spear, Transform(ent).Coordinates);
            _hands.TryPickup(ent, spear, hand, false, handsComp: hands);
            return;
        }

        var newSpear = Spawn(spearAction.SpearProto, Transform(ent).Coordinates);
        if (!_hands.TryForcePickupAnyHand(ent, newSpear, false, hands))
        {
            QueueDel(newSpear);
            return;
        }

        spearAction.CreatedSpear = newSpear;
        EnsureComp<IceSpearComponent>(newSpear).ActionId = args.Action;
    }

    private void OnRustCharge(Entity<HereticComponent> ent, ref EventHereticRustCharge args)
    {
        if (!args.Target.IsValid(EntityManager) || !TryUseAbility(ent, args))
            return;

        var xform = Transform(ent);

        if (!IsTileRust(xform.Coordinates, out _))
        {
            Popup.PopupClient(Loc.GetString("heretic-ability-fail-tile-underneath-not-rusted"), ent, ent);
            return;
        }

        var ourCoords = _transform.ToMapCoordinates(args.Target);
        var targetCoords = _transform.GetMapCoordinates(ent, xform);

        if (ourCoords.MapId != targetCoords.MapId)
            return;

        var dir = ourCoords.Position - targetCoords.Position;

        if (dir.LengthSquared() < 0.001f)
            return;

        _standing.Stand(ent);
        EnsureComp<RustChargeComponent>(ent);
        EnsureComp<RustObjectsInRadiusComponent>(ent);
        _throw.TryThrow(ent, dir.Normalized() * args.Distance, args.Speed, playSound: false, doSpin: false);

        args.Handled = true;
    }
}
