using Content.Goobstation.Shared.Terror.Components;
using Content.Goobstation.Shared.Terror.Events;
using Content.Shared.Damage;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Events;

namespace Content.Goobstation.Shared.Terror.Systems;

/// <summary>
/// Charge up, dash, break first structure hit or knock down and damage first living thing hit.
/// </summary>
public sealed class TerrorChargeSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TerrorChargeComponent, TerrorChargeEvent>(OnCharge);
        SubscribeLocalEvent<TerrorChargeComponent, StartCollideEvent>(OnCollide);
        SubscribeLocalEvent<TerrorChargeComponent, StopThrowEvent>(OnStopThrow);
    }

    private void OnCharge(Entity<TerrorChargeComponent> ent, ref TerrorChargeEvent args)
    {
        _popup.PopupPredicted(Loc.GetString("terror-charge"), ent.Owner, ent.Owner);

        ent.Comp.IsCharging = true;
        Dirty(ent);

        var from = Transform(ent.Owner).Coordinates;
        var direction = args.Target.ToMap(EntityManager, _transform).Position - _transform.GetMapCoordinates(ent.Owner).Position;

        if (direction.Length() > ent.Comp.DashDistance)
        {
            direction = direction.Normalized() * ent.Comp.DashDistance;
        }

        var throwTarget = from.Offset(direction);
        _throwing.TryThrow(ent.Owner, throwTarget, ent.Comp.DashSpeed);

        _audio.PlayPredicted(ent.Comp.ChargeSound, ent.Owner, ent.Owner);

        args.Handled = true;
    }

    private void OnCollide(Entity<TerrorChargeComponent> ent, ref StartCollideEvent args)
    {
        if (!ent.Comp.IsCharging)
        {
            return;
        }

        if (HasComp<MobStateComponent>(args.OtherEntity))
        {
            _stun.TryAddStunDuration(args.OtherEntity, ent.Comp.TargetStun);
            _stun.TryKnockdown(args.OtherEntity, ent.Comp.TargetKnockdown, true);
            _damageable.TryChangeDamage(args.OtherEntity, ent.Comp.TargetDamage, origin: ent.Owner);
        }
        else
        {
            _damageable.TryChangeDamage(args.OtherEntity, ent.Comp.StructureDamage, ignoreResistances: true, origin: ent.Owner);
        }

        _audio.PlayPredicted(ent.Comp.ImpactSound, ent.Owner, ent.Owner);

        ent.Comp.IsCharging = false;
        Dirty(ent);
    }

    private void OnStopThrow(Entity<TerrorChargeComponent> ent, ref StopThrowEvent args)
    {
        ent.Comp.IsCharging = false;
        Dirty(ent);
    }
}
