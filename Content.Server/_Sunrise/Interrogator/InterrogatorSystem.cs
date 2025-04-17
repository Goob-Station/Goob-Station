using System.Linq;
using Content.Server.Administration.Logs;
using Content.Server.Power.EntitySystems;
using Content.Shared._Sunrise.Interrogator;
using Content.Shared.Climbing.Systems;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.DragDrop;
using Content.Shared.Implants.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Power;
using Content.Shared.Verbs;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;

namespace Content.Server._Sunrise.Interrogator
{
    public sealed class InterrogatorSystem : SharedInterrogatorSystem
    {
        [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
        [Dependency] private readonly IAdminLogManager _adminLogger = default!;
        [Dependency] private readonly PowerReceiverSystem _powerReceiverSystem = default!;
        [Dependency] private readonly ClimbSystem _climbSystem = default!;
        [Dependency] private readonly SharedContainerSystem _container = default!;
        [Dependency] private readonly MobStateSystem _mobState = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly SharedPointLightSystem _light = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<InterrogatorComponent, ComponentInit>(OnComponentInit);
            SubscribeLocalEvent<InterrogatorComponent, GetVerbsEvent<AlternativeVerb>>(AddAlternativeVerbs);
            SubscribeLocalEvent<InterrogatorComponent, DragDropTargetEvent>(HandleDragDropOn);
            SubscribeLocalEvent<InterrogatorComponent, InterrogatorDragFinished>(OnDragFinished);
            SubscribeLocalEvent<InterrogatorComponent, PowerChangedEvent>(OnPowerChanged);
            SubscribeLocalEvent<InterrogatorComponent, AnchorStateChangedEvent>(OnAnchorChanged);
            SubscribeLocalEvent<InterrogatorComponent, EntRemovedFromContainerMessage>(OnEjected);
            SubscribeLocalEvent<ActiveInterrogatorComponent, ComponentStartup>(OnExtractStart);
            SubscribeLocalEvent<ActiveInterrogatorComponent, ComponentShutdown>(OnExtractStop);
        }

        private void OnAnchorChanged(EntityUid uid, InterrogatorComponent component, ref AnchorStateChangedEvent args)
        {
            if (!args.Anchored)
                _container.EmptyContainer(component.BodyContainer);
        }

        private void StopExtracting(Entity<InterrogatorComponent> ent)
        {
            RemCompDeferred<ActiveInterrogatorComponent>(ent);
        }

        private void OnExtractStart(Entity<ActiveInterrogatorComponent> ent, ref ComponentStartup args)
        {
            if (!TryComp<InterrogatorComponent>(ent, out var interrogatorComponent))
                return;
            //SetAppearance(ent.Owner, MicrowaveVisualState.Cooking, microwaveComponent);

            interrogatorComponent.ExtractionProgress = 0;
            interrogatorComponent.PlayingStream =
                _audio.PlayPvs(interrogatorComponent.ExtractingSound, ent, AudioParams.Default.WithLoop(true).WithMaxDistance(5))?.Entity;
            UpdateAppearance(ent);
        }

        private void OnExtractStop(Entity<ActiveInterrogatorComponent> ent, ref ComponentShutdown args)
        {
            if (!TryComp<InterrogatorComponent>(ent, out var interrogatorComponent))
                return;

            interrogatorComponent.ExtractionProgress = 0;
            //SetAppearance(ent.Owner, MicrowaveVisualState.Idle, microwaveComponent);
            interrogatorComponent.PlayingStream = _audio.Stop(interrogatorComponent.PlayingStream);
            _audio.PlayPvs(interrogatorComponent.ExtractDoneSound, ent);
        }

        private void OnEjected(Entity<InterrogatorComponent> interrogator, ref EntRemovedFromContainerMessage args)
        {
            StopExtracting(interrogator);
        }

        private void OnPowerChanged(Entity<InterrogatorComponent> entity, ref PowerChangedEvent args)
        {
            // Needed to avoid adding/removing components on a deleted entity
            if (Terminating(entity))
            {
                return;
            }

            if (!args.Powered)
            {
                StopExtracting(entity);
                EjectBody(entity.Owner, entity.Comp);

                if (_light.TryGetLight(entity.Owner, out var light))
                {
                    _light.SetEnabled(entity.Owner, false, light);
                }
            }

            UpdateAppearance(entity.Owner, entity.Comp);
        }

        public override void Update(float frameTime)
        {
            var query = EntityQueryEnumerator<ActiveInterrogatorComponent, InterrogatorComponent>();
            while (query.MoveNext(out var uid, out var _, out var interrogator))
            {
                if (!_powerReceiverSystem.IsPowered(uid))
                    continue;

                if (interrogator.BodyContainer.ContainedEntity == null)
                    continue;

                if (_mobState.IsDead(interrogator.BodyContainer.ContainedEntity.Value))
                {
                    StopExtracting((uid, interrogator));
                    EjectBody(uid, interrogator);
                    continue;
                }

                if (interrogator.BodyContainer.ContainedEntity == null)
                    continue;

                interrogator.ExtractionProgress += frameTime;
                if (interrogator.ExtractionProgress < interrogator.ExtractionTime)
                    continue;

                EjectImplants(interrogator.BodyContainer.ContainedEntity.Value);
                StopExtracting((uid, interrogator));
                EjectBody(uid, interrogator);
            }
        }

        private void EjectImplants(EntityUid target)
        {
            if (_container.TryGetContainer(target, ImplanterComponent.ImplantSlotId, out var implantContainer))
            {
                var implantCompQuery = GetEntityQuery<SubdermalImplantComponent>();

                // Create a copy of the ContainedEntities list
                var implants = implantContainer.ContainedEntities.ToList();

                foreach (var implant in implants)
                {
                    if (!implantCompQuery.TryGetComponent(implant, out var implantComp))
                        continue;

                    // Don't remove a permanent implant and look for the next that can be drawn
                    if (!_container.CanRemove(implant, implantContainer))
                    {
                        continue;
                    }

                    _container.Remove(implant, implantContainer);
                }
            }
        }

        protected override EntityUid? EjectBody(EntityUid uid, InterrogatorComponent? component)
        {
            if (!Resolve(uid, ref component))
                return null;
            if (component.BodyContainer.ContainedEntity is not { Valid: true } contained)
                return null;
            base.EjectBody(uid, component);
            _climbSystem.ForciblySetClimbing(contained, uid);
            return contained;
        }

        private void OnDragFinished(Entity<InterrogatorComponent> entity, ref InterrogatorDragFinished args)
        {
            if (args.Cancelled || args.Handled || args.Args.Target == null)
                return;

            if (InsertBody(entity.Owner, args.Args.Target.Value, entity.Comp))
            {
                _adminLogger.Add(LogType.Action, LogImpact.Medium,
                    $"{ToPrettyString(args.User)} inserted {ToPrettyString(args.Args.Target.Value)} into {ToPrettyString(entity.Owner)}");
            }
            args.Handled = true;
        }

        private void HandleDragDropOn(Entity<InterrogatorComponent> entity, ref DragDropTargetEvent args)
        {
            if (entity.Comp.BodyContainer.ContainedEntity != null)
                return;

            var doAfterArgs = new DoAfterArgs(EntityManager, args.User, entity.Comp.EntryDelay, new InterrogatorDragFinished(), entity, target: args.Dragged, used: entity)
            {
                BreakOnDamage = true,
                BreakOnMove = true,
                NeedHand = false,
            };
            _doAfterSystem.TryStartDoAfter(doAfterArgs);
            args.Handled = true;
        }
    }
}
