using System.Linq;
using System.Numerics;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;

namespace Content.Shared._White.Xenomorphs.TailLash;

public sealed class SharedTailLashSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly SharedMeleeWeaponSystem _meleeWeapon = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TailLashComponent, MapInitEvent>(OnTailLashMapInit);
        SubscribeLocalEvent<TailLashComponent, ComponentShutdown>(OnTailLashShutdown);
        SubscribeLocalEvent<TailLashComponent, TailLashActionEvent>(OnTailLashActionEvent);
    }

    private void OnTailLashMapInit(EntityUid uid, TailLashComponent component, MapInitEvent args) =>
        _actions.AddAction(uid, ref component.TailLashAction, component.TailLashActionId);

    private void OnTailLashShutdown(EntityUid uid, TailLashComponent component, ComponentShutdown args) =>
        _actions.RemoveAction(uid, component.TailLashAction);

    private void OnTailLashActionEvent(EntityUid uid, TailLashComponent component, ref TailLashActionEvent args)
    {
        if (args.Handled || !_actionBlocker.CanAttack(uid) || !TryComp(uid, out TransformComponent? transform))
            return;

        if (TryComp<MeleeWeaponComponent>(uid, out var meleeWeapon))
        {
            if (_timing.CurTime < meleeWeapon.NextAttack)
                return;

            meleeWeapon.NextAttack = _timing.CurTime + TimeSpan.FromSeconds(1);
            Dirty(uid, meleeWeapon);
        }

        var userCoords = _transform.GetMapCoordinates(uid);
        var targetCoords = _transform.ToMapCoordinates(args.Target);

        var range = component.TailRange.Float();
        var box = new Box2(userCoords.Position.X - 0.10f, userCoords.Position.Y, userCoords.Position.X + 0.10f, userCoords.Position.Y + range);

        var matrix = Vector2.Transform(targetCoords.Position, _transform.GetInvWorldMatrix(transform));
        var rotation = _transform.GetWorldRotation(uid).RotateVec(-matrix).ToWorldAngle();
        var boxRotated = new Box2Rotated(box, rotation, userCoords.Position);

        var leftRay = new CollisionRay(boxRotated.BottomLeft, (boxRotated.TopLeft - boxRotated.BottomLeft).Normalized(), SharedMeleeWeaponSystem.AttackMask);
        var rightRay = new CollisionRay(boxRotated.BottomRight, (boxRotated.TopRight - boxRotated.BottomRight).Normalized(), SharedMeleeWeaponSystem.AttackMask);

        bool Ignored(EntityUid predicate)
        {
            if (predicate == uid || !HasComp<MobStateComponent>(predicate))
                return true;

            return HasComp<XenomorphComponent>(predicate);
        }

        var intersect = _physics.IntersectRayWithPredicate(transform.MapID, leftRay, range, Ignored, false);
        intersect = intersect.Concat(_physics.IntersectRayWithPredicate(transform.MapID, rightRay, range, Ignored, false));
        var results = intersect.Select(r => r.HitEntity).ToHashSet();

        _interaction.DoContactInteraction(uid, uid);

        var hitEntities = results.Where(result => _interaction.InRangeUnobstructed(uid, result, range: range)).ToList();
        var hitEvent = new MeleeHitEvent(hitEntities, uid, uid, component.TailDamage, null, args.Target);
        RaiseLocalEvent(uid, hitEvent);

        foreach (var hit in hitEntities)
        {
            _interaction.DoContactInteraction(uid, hit);

            var attackedEv = new AttackedEvent(uid, hit, args.Target);
            RaiseLocalEvent(hit, attackedEv);

            var modifiedDamage = DamageSpecifier.ApplyModifierSets(component.TailDamage + hitEvent.BonusDamage + attackedEv.BonusDamage, hitEvent.ModifiersList);
            _damageable.TryChangeDamage(hit, modifiedDamage, origin:uid);

            if (component.Inject == null || !_solutionContainer.TryGetInjectableSolution(hit, out var solutionEnt, out _))
                continue;

            foreach (var (reagent, amount) in component.Inject)
                _solutionContainer.TryAddReagent(solutionEnt.Value, reagent, amount);
        }

        var localPos = transform.LocalRotation.RotateVec(matrix);
        _meleeWeapon.DoLunge(uid, uid, rotation, localPos, component.TailAnimationId, new Angle(0), false);
        _audio.PlayPredicted(component.HitSound, uid, uid);

        var attackEv = new MeleeAttackEvent(uid);
        RaiseLocalEvent(uid, ref attackEv);

        args.Handled = true;
    }
}
