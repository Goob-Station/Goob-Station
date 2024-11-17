using Content.Server.Body.Components;
using Content.Server.Ghost.Components;
using Content.Shared.Body.Components;
using Content.Shared.Body.Events;
using Content.Shared._Shitmed.Body.Organ; // Shitmed Change
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Pointing;

namespace Content.Server.Body.Systems
{
    public sealed class BrainSystem : EntitySystem
    {
        [Dependency] private readonly SharedMindSystem _mindSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<BrainComponent, OrganAddedToBodyEvent>(HandleAddition);
        // Shitmed Change Start
            SubscribeLocalEvent<BrainComponent, OrganRemovedFromBodyEvent>(HandleRemoval);
            SubscribeLocalEvent<BrainComponent, PointAttemptEvent>(OnPointAttempt);
        }

        private void HandleRemoval(EntityUid uid, BrainComponent _, ref OrganRemovedFromBodyEvent args)
        {
            if (TerminatingOrDeleted(uid) || TerminatingOrDeleted(args.OldBody))
                return;

            // Prevents revival, should kill the user within a given timespan too.
            EnsureComp<DebrainedComponent>(args.OldBody);
            HandleMind(uid, args.OldBody);
        }
        private void HandleAddition(EntityUid uid, BrainComponent _, ref OrganAddedToBodyEvent args)
        {
            if (TerminatingOrDeleted(uid) || TerminatingOrDeleted(args.Body))
                return;

            RemComp<DebrainedComponent>(args.Body);
            HandleMind(args.Body, uid);
        }
        // Shitmed Change End

        private void HandleMind(EntityUid newEntity, EntityUid oldEntity)
        {
            if (TerminatingOrDeleted(newEntity) || TerminatingOrDeleted(oldEntity))
                return;

            EnsureComp<MindContainerComponent>(newEntity);
            EnsureComp<MindContainerComponent>(oldEntity);

            var ghostOnMove = EnsureComp<GhostOnMoveComponent>(newEntity);
            if (HasComp<BodyComponent>(newEntity))
                ghostOnMove.MustBeDead = true;

            if (!_mindSystem.TryGetMind(oldEntity, out var mindId, out var mind))
                return;

            _mindSystem.TransferTo(mindId, newEntity, mind: mind);
        }

        private void OnPointAttempt(Entity<BrainComponent> ent, ref PointAttemptEvent args)
        {
            args.Cancel();
        }
    }
}
