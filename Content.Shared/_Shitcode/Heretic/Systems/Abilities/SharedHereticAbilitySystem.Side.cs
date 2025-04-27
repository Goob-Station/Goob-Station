using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Heretic;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs.Components;
using Content.Shared.Standing;
using Content.Shared.StatusEffect;
using Content.Shared.Throwing;
using Robust.Shared.Physics.Events;

namespace Content.Shared._Shitcode.Heretic.Systems.Abilities;

public abstract partial class SharedHereticAbilitySystem
{
    protected virtual void SubscribeSide()
    {
        SubscribeLocalEvent<HereticComponent, EventHereticRustCharge>(OnRustCharge);
        SubscribeLocalEvent<RustChargeComponent, StartCollideEvent>(OnCollide);
        SubscribeLocalEvent<RustChargeComponent, PreventCollideEvent>(OnPreventCollide);
        SubscribeLocalEvent<RustChargeComponent, LandEvent>(OnLand);
        SubscribeLocalEvent<RustChargeComponent, DownAttemptEvent>(OnDownAttempt);
        SubscribeLocalEvent<RustChargeComponent, InteractionAttemptEvent>(OnInteractAttempt);
        SubscribeLocalEvent<RustChargeComponent, BeforeStatusEffectAddedEvent>(OnBeforeRustChargeStatusEffect);
    }

    private void OnBeforeRustChargeStatusEffect(Entity<RustChargeComponent> ent, ref BeforeStatusEffectAddedEvent args)
    {
        if (args.Key == "KnockedDown")
            args.Cancelled = true;
    }

    private void OnInteractAttempt(Entity<RustChargeComponent> ent, ref InteractionAttemptEvent args)
    {
        args.Cancelled = true;
    }

    private void OnDownAttempt(Entity<RustChargeComponent> ent, ref DownAttemptEvent args)
    {
        args.Cancel();
    }

    private void OnPreventCollide(Entity<RustChargeComponent> ent, ref PreventCollideEvent args)
    {
        if (!args.OtherFixture.Hard)
            return;

        var other = args.OtherEntity;

        if (!HasComp<DamageableComponent>(other) || _tag.HasTag(other, "IgnoreImmovableRod") ||
            ent.Comp.DamagedEntities.Contains(other))
            args.Cancelled = true;
    }

    private void OnLand(Entity<RustChargeComponent> ent, ref LandEvent args)
    {
        RemCompDeferred(ent.Owner, ent.Comp);
    }

    private void OnCollide(Entity<RustChargeComponent> ent, ref StartCollideEvent args)
    {
        if (!args.OtherFixture.Hard)
            return;

        var other = args.OtherEntity;

        if (ent.Comp.DamagedEntities.Contains(other))
            return;

        _audio.PlayPredicted(ent.Comp.HitSound, ent, ent);

        ent.Comp.DamagedEntities.Add(other);

        // I would check for DamageableComponent but it is in server for whatever reason, also prevent collide handles that
        if (!TryComp(other, out DamageableComponent? damageable) || _tag.HasTag(other, "IgnoreImmovableRod"))
            return;

        // Damage mobs
        if (HasComp<MobStateComponent>(other))
        {
            _stun.KnockdownOrStun(other, ent.Comp.KnockdownTime, true);

            _damageable.TryChangeDamage(other,
                ent.Comp.Damage,
                false,
                true,
                damageable,
                targetPart: TargetBodyPart.Torso);

            return;
        }

        // Delete structures
        if (_net.IsServer)
            Del(other);
    }

    private void OnRustCharge(Entity<HereticComponent> ent, ref EventHereticRustCharge args)
    {
        if (!args.Target.IsValid(EntityManager) || !TryUseAbility(ent, args))
            return;

        var xform = Transform(ent);

        if (!IsTileRust(xform.Coordinates, out _))
        {
            Popup.PopupPredicted(Loc.GetString("heretic-ability-fail-tile-underneath-not-rusted"), ent, ent);
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
        _throw.TryThrow(ent, dir.Normalized() * args.Distance, args.Speed, playSound: false, doSpin: false);

        args.Handled = true;
    }
}
