using Content.Shared._ES.Camera;
using Content.Shared.Administration.Logs;
using Content.Shared.Camera;
using Content.Shared.Coordinates;
using Content.Shared.Damage.Components;
using Content.Shared.Database;
using Content.Shared.Effects;
using Content.Shared.Mobs.Components;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Network;
using Robust.Shared.Physics.Components;
using Robust.Shared.Player;

namespace Content.Shared.Damage.Systems;

/// <summary>
/// Trauma - thrown item hits moved out of server so they can be predicted.
/// </summary>
public abstract partial class SharedDamageOtherOnHitSystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly SharedCameraRecoilSystem _recoil = default!;
    [Dependency] private readonly SharedColorFlashEffectSystem _color = default!;
    [Dependency] private readonly ESScreenshakeSystem _shake = default!;

    private static readonly ESScreenshakeParameters HitShake = new(0.35f, 1.4f, 0.014f);

    private readonly List<EntityUid> _target = new(1);

    private EntityQuery<MobStateComponent> _mobQuery;
    private EntityQuery<PhysicsComponent> _physicsQuery;

    private void InitializeTrauma()
    {
        _mobQuery = GetEntityQuery<MobStateComponent>();
        _physicsQuery = GetEntityQuery<PhysicsComponent>();

        SubscribeLocalEvent<DamageOtherOnHitComponent, ThrowDoHitEvent>(OnDoHit);
    }

    private void OnDoHit(EntityUid uid, DamageOtherOnHitComponent component, ThrowDoHitEvent args)
    {
        if (TerminatingOrDeleted(args.Target))
            return;

        if (args.Target == args.Component.Thrower) // Goobstation - Mjolnir
            return;

        var dmg = _damageable.ChangeDamage(args.Target, component.Damage * _damageable.UniversalThrownDamageModifier, component.IgnoreResistances, origin: args.Component.Thrower);

        // For stuff that cares about it being attacked. GOOBSTATION!!!
        var attackedEvent = new AttackedEvent(args.Thrown, uid, args.Target.ToCoordinates());
        RaiseLocalEvent(args.Target, attackedEvent);

        // Log damage only for mobs. Useful for when people throw spears at each other, but also avoids log-spam when explosions send glass shards flying.
        if (_mobQuery.HasComp(args.Target))
            _adminLogger.Add(LogType.ThrowHit, $"{ToPrettyString(args.Target):target} received {dmg.GetTotal():damage} damage from collision");

        // Goob - not predicted so you know if it flashes its 100% a hit, same as projectiles
        if (!dmg.Empty && _net.IsServer)
        {
            _target.Clear();
            _target.Add(args.Target);
            _color.RaiseEffect(Color.Red, _target, Filter.Pvs(args.Target, entityManager: EntityManager));
        }

        _gun.PlayImpactSound(args.Target, dmg, null, false, args.Component.Thrower); // Goob - thrower predicted it
        // Goob - server sends the kick to the target, clients would otherwise kick again on every reprediction
        if (_net.IsServer && _physicsQuery.TryComp(uid, out var body) && body.LinearVelocity.LengthSquared() > 0f)
        {
            var direction = body.LinearVelocity.Normalized();
            _recoil.KickCamera(args.Target, direction * 0.1f);
            _shake.Screenshake(args.Target, HitShake, null);
        }
    }
}
