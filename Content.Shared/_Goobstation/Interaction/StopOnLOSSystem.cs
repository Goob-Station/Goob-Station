using System.Linq;
using Content.Shared._Goobstation.Interaction.Components;
using Content.Shared.Movement.Events;
using Content.Shared.Interaction.Events;
using Content.Shared.Maps;
using Content.Shared.ActionBlocker;
using Content.Shared.Mobs.Components;
using Content.Shared.NPC.Systems;
using Content.Shared.Mind;
using Content.Shared.Examine;

namespace Content.Shared._Goobstation.Interaction;

public sealed class SharedStopOnLOSSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly ActionBlockerSystem _blocker = default!;
    [Dependency] private readonly NpcFactionSystem _faction = default!;
    [Dependency] private readonly ExamineSystemShared _examine = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<StopOnLOSComponent, UpdateCanMoveEvent>(OnAttempt);
        SubscribeLocalEvent<StopOnLOSComponent, AttackAttemptEvent>(OnAttempt);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<StopOnLOSComponent>();
        while(query.MoveNext(out var entity, out var comp))
        {
            List<EntityUid> observers = new List<EntityUid>();

            foreach (var ent in _faction.GetNearbyHostiles(entity, comp.SightRange))
            {
                if (!TryComp<MobStateComponent>(ent, out var state) || HasComp<StopOnLOSComponent>(ent))
                    continue;
                if(_mind.TryGetMind(ent, out var mindId, out var mind) && state.CurrentState == Shared.Mobs.MobState.Alive)
                    observers.Add(ent);
            }

            if(!observers.Any())
                comp.CanMove = true;

            foreach (var target in observers)
            {
                var direction = _transform.GetWorldPosition(entity) - _transform.GetWorldPosition(target);

                direction.Normalize();

                var difference = Math.Min(
                    Math.Abs(direction.ToWorldAngle().Degrees - _transform.GetWorldPositionRotation(target).WorldRotation.Degrees),
                    Math.Abs(_transform.GetWorldPositionRotation(target).WorldRotation.Degrees - direction.ToWorldAngle().Degrees));

                var notoccluded = _examine.InRangeUnOccluded(target, entity, comp.SightRange, null);

                if (difference < comp.SightAngle && notoccluded && comp.CanMove)
                {
                    comp.CanMove = false;
                    break;
                }
                else if (difference >= comp.SightAngle && !comp.CanMove || !notoccluded && !comp.CanMove)
                {
                    comp.CanMove = true;
                }
            }

            _blocker.UpdateCanMove(entity);
        }
    }

    private void OnAttempt(EntityUid uid, StopOnLOSComponent comp, CancellableEntityEventArgs args)
    {
        if(!comp.CanMove)
            args.Cancel();
    }
}
