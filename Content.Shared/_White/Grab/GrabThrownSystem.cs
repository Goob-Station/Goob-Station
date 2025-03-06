using Content.Shared.Damage.Systems;
using Content.Shared.Damage;
using Content.Shared.Effects;
using Content.Shared.Throwing;
using Robust.Shared.Network;
using Robust.Shared.Physics.Events;
using Robust.Shared.Player;
using System.Numerics;
using Content.Shared._White.Standing;
using Content.Shared.Standing;

namespace Content.Shared._White.Grab;

public sealed class GrabThrownSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedColorFlashEffectSystem _color = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly INetManager _netMan = default!;
    [Dependency] private readonly SharedLayingDownSystem _layingDown = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GrabThrownComponent, StartCollideEvent>(HandleCollide);
        SubscribeLocalEvent<GrabThrownComponent, StopThrowEvent>(OnStopThrow);
    }

    private void HandleCollide(EntityUid uid, GrabThrownComponent component, ref StartCollideEvent args)
    {
        if (_netMan.IsClient) // To avoid effect spam
            return;

        if (!HasComp<ThrownItemComponent>(uid))
        {
            RemComp<GrabThrownComponent>(uid);
            return;
        }

        if (component.IgnoreEntity.Contains(args.OtherEntity))
            return;

        if (!HasComp<DamageableComponent>(uid))
            RemComp<GrabThrownComponent>(uid);

        component.IgnoreEntity.Add(args.OtherEntity);

        var speed = args.OurBody.LinearVelocity.Length();

        if (component.StaminaDamageOnCollide != null)
            _stamina.TakeStaminaDamage(uid, component.StaminaDamageOnCollide.Value);

        var damageScale = speed;

        if (component.WallDamageOnCollide != null)
            _damageable.TryChangeDamage(args.OtherEntity, component.WallDamageOnCollide * damageScale);

        _layingDown.TryLieDown(args.OtherEntity, behavior: DropHeldItemsBehavior.AlwaysDrop);

        _color.RaiseEffect(Color.Red, new List<EntityUid>() { uid }, Filter.Pvs(uid, entityManager: EntityManager));
    }

    private void OnStopThrow(EntityUid uid, GrabThrownComponent comp, StopThrowEvent args)
    {
        if (comp.DamageOnCollide != null)
            _damageable.TryChangeDamage(uid, comp.DamageOnCollide);

        if (HasComp<GrabThrownComponent>(uid))
            RemComp<GrabThrownComponent>(uid);
    }

    /// <summary>
    /// Throwing entity to the direction and ensures GrabThrownComponent with params
    /// </summary>
    /// <param name="uid">Entity to throw</param>
    /// <param name="thrower">Entity that throws</param>
    /// <param name="vector">Direction</param>
    /// <param name="staminaDamage">Stamina damage on collide</param>
    /// <param name="damageToUid">Damage to entity on collide</param>
    /// <param name="damageToWall">Damage to wall or anything that was hit by entity</param>
    public void Throw(
        EntityUid uid,
        EntityUid thrower,
        Vector2 vector,
        float? staminaDamage = null,
        DamageSpecifier? damageToUid = null,
        DamageSpecifier? damageToWall = null)
    {
        _throwing.TryThrow(uid, vector, 5f, animated: false);

        var comp = EnsureComp<GrabThrownComponent>(uid);
        comp.StaminaDamageOnCollide = staminaDamage;
        comp.DamageOnCollide = damageToUid;
        comp.WallDamageOnCollide = damageToWall;
        comp.IgnoreEntity.Add(thrower);

        _layingDown.TryLieDown(uid, behavior: DropHeldItemsBehavior.AlwaysDrop);
    }
}
