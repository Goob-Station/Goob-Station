using System.Numerics;
using Content.Shared._Goobstation.Fishing.Components;
using Content.Shared._Goobstation.Fishing.Events;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Events;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Throwing;
using Robust.Shared.Network;
using Robust.Shared.Physics.Events;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Content.Shared.Actions;

namespace Content.Shared._Goobstation.Fishing.Systems;

/// <summary>
/// This handles... da fish and fishing process itself
/// </summary>
public abstract class SharedFishingSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    private EntityQuery<ActiveFisherComponent> _fisherQuery;
    private EntityQuery<ActiveFishingSpotComponent> _activeFishSpotQuery;
    private EntityQuery<FishingSpotComponent> _fishSpotQuery;
    private EntityQuery<FishingRodComponent> _fishRodQuery;
    private EntityQuery<FishingLureComponent> _fishFloatQuery;

    public override void Initialize()
    {
        base.Initialize();

        _fisherQuery = GetEntityQuery<ActiveFisherComponent>();
        _activeFishSpotQuery = GetEntityQuery<ActiveFishingSpotComponent>();
        _fishSpotQuery = GetEntityQuery<FishingSpotComponent>();
        _fishRodQuery = GetEntityQuery<FishingRodComponent>();
        _fishFloatQuery = GetEntityQuery<FishingLureComponent>();

        SubscribeLocalEvent<FishingRodComponent, MapInitEvent>(OnFishingRodInit);
        SubscribeLocalEvent<FishingRodComponent, GetItemActionsEvent>(OnGetActions);

        SubscribeLocalEvent<FishingRodComponent, UseInHandEvent>(OnFishingInteract);
        SubscribeLocalEvent<FishingRodComponent, ThrowFishingLureActionEvent>(OnThrowFloat);
        SubscribeLocalEvent<FishingRodComponent, PullFishingLureActionEvent>(OnPullFloat);

        SubscribeLocalEvent<FishingLureComponent, StartCollideEvent>(OnFloatCollide);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        UpdateFishing(frameTime);
    }

    protected void UpdateFishing(float frameTime)
    {
        var currentTime = _timing.CurTime;
        var activeFishers = EntityQueryEnumerator<ActiveFisherComponent>();
        while (activeFishers.MoveNext(out var fisher, out var fisherComp))
        {
            if (fisherComp.StartTime != null && fisherComp.EndTime != null)
            {
                var elapsedTime = currentTime - fisherComp.StartTime;
                var totalDuration = fisherComp.EndTime - fisherComp.StartTime;
                fisherComp.TotalProgress = (float) (elapsedTime.Value.TotalSeconds / totalDuration.Value.TotalSeconds);
            }

            // Get fishing rod, then float, then spot... ReCurse.
            if (!_fishRodQuery.TryComp(fisherComp.FishingRod, out var fishingRodComp))
                continue;
            if (!_fishFloatQuery.TryComp(fishingRodComp.FishingLure, out var fishingFloatComp))
                continue;
            if (!_activeFishSpotQuery.TryComp(fishingFloatComp.AttachedEntity, out var activeSpotComp))
                continue;

            var fishRod = fisherComp.FishingRod;
            var fishingLure = fishingRodComp.FishingLure.Value;
            var fishSpot = fishingFloatComp.AttachedEntity.Value;

            if (fisherComp.StartTime == null || fisherComp.EndTime == null)
            {
                fisherComp.StartTime = currentTime;
                fisherComp.EndTime = currentTime + TimeSpan.FromSeconds(1f / Math.Abs(activeSpotComp.FishDifficulty));
            }

            if (!_hands.IsHolding(fisher, fishRod))
            {
                _popup.PopupEntity(Loc.GetString("fishing-progress-lost-rod", ("ent", Name(fishRod))), fisher, fisher);

                // Cleanup entities and their connections
                RemComp(fisher, fisherComp);
                RemComp(fishSpot, activeSpotComp);
                QueueDel(fishingLure);
                _actions.RemoveAction(fishingRodComp.PullLureActionEntity);
                _actions.AddAction(fisher, ref fishingRodComp.ThrowLureActionEntity, fishingRodComp.ThrowLureActionId, fishRod);
                fishingRodComp.FishingLure = null;
                continue;
            }

            if (fisherComp.TotalProgress < 0f)
            {
                // It's over
                _popup.PopupEntity(Loc.GetString("fishing-progress-fail"), fisher, fisher);

                // Cleanup entities and their connections
                RemComp(fisher, fisherComp);
                RemComp(fishSpot, activeSpotComp);
                QueueDel(fishingLure);
                _actions.RemoveAction(fishingRodComp.PullLureActionEntity);
                _actions.AddAction(fisher, ref fishingRodComp.ThrowLureActionEntity, fishingRodComp.ThrowLureActionId, fishRod);
                fishingRodComp.FishingLure = null;
                continue;
            }

            // Fish fighting logic
            if (_timing.IsFirstTimePredicted)
            {
                var rand = new System.Random((int) _timing.CurTick.Value);

                if (fisherComp.StartTime + TimeSpan.FromSeconds(1f) <= _timing.CurTime && fisherComp.NextStruggle == null)
                    fisherComp.NextStruggle = _timing.CurTime + TimeSpan.FromSeconds(rand.NextFloat(0.5f, 10) * activeSpotComp.FishDifficulty);

                if (fisherComp.NextStruggle != null && fisherComp.NextStruggle <= _timing.CurTime)
                {
                    fisherComp.EndTime += TimeSpan.FromSeconds(rand.NextFloat(0, 0.01f) / Math.Abs(activeSpotComp.FishDifficulty));
                    fisherComp.NextStruggle = _timing.CurTime + TimeSpan.FromSeconds(rand.NextFloat(0.5f, 10) * activeSpotComp.FishDifficulty);
                }
            }

            if (fisherComp.TotalProgress >= 1.0f)
            {
                // We're spawning the fish only on the server, because I don't want to deal with networking just for this
                if (_net.IsServer)
                {
                    var fishIds = activeSpotComp.FishList.GetSpawns(_random.GetRandom(), EntityManager, _proto);
                    var position = Transform(fishingFloatComp.AttachedEntity.Value).Coordinates;
                    foreach (var fishId in fishIds)
                    {
                        var fish = Spawn(fishId, position);
                        // Throw da fish back to the player because it looks funny
                        var direction = _transform.GetWorldPosition(fisher) - _transform.GetWorldPosition(fish);
                        var length = direction.Length();
                        var distance = Math.Clamp(length, 0.5f, 15f);
                        direction *= distance / length;

                        _throwing.TryThrow(fish, direction, 7f);
                    }

                    // Message
                    _popup.PopupEntity(Loc.GetString("fishing-progress-success"), fisher, fisher);
                }

                // Cleanup entities and their connections
                QueueDel(fishingLure);
                RemComp(fisher, fisherComp);
                RemComp(fishSpot, activeSpotComp);
                _actions.RemoveAction(fishingRodComp.PullLureActionEntity);
                _actions.AddAction(fisher, ref fishingRodComp.ThrowLureActionEntity, fishingRodComp.ThrowLureActionId, fishRod);
                fishingRodComp.FishingLure = null;
            }
        }

        var fishingSpots = EntityQueryEnumerator<ActiveFishingSpotComponent>();
        while (fishingSpots.MoveNext(out var activeSpotComp))
        {
            activeSpotComp.Accumulator -= frameTime;
            if (activeSpotComp.Accumulator > 0f || activeSpotComp.IsActive)
                continue;

            // Trigger start of the fishing process
            if (TerminatingOrDeleted(activeSpotComp.AttachedFishingLure))
                continue;

            // Get fishing lure, then rod, then player... ReCurse.
            if (!_fishFloatQuery.TryComp(activeSpotComp.AttachedFishingLure, out var fishingFloatComp) ||
                !_fishRodQuery.TryComp(fishingFloatComp.FishingRod, out var fishRodComp))
                continue;

            var fishRod = fishingFloatComp.FishingRod;
            var fisher = Transform(fishingFloatComp.FishingRod).ParentUid;

            if (!_hands.IsHolding(fisher, fishRod) ||
                !HasComp<ActorComponent>(fisher))
                continue;

            var activeFisher = EnsureComp<ActiveFisherComponent>(fisher);
            activeFisher.FishingRod = fishRod;
            activeFisher.ProgressPerUse *= fishRodComp.Efficiency;

            _popup.PopupEntity(Loc.GetString("fishing-progress-start"), fisher, fisher);
            activeSpotComp.IsActive = true;
        }
    }

    private void OnFishingInteract(EntityUid uid, FishingRodComponent component, UseInHandEvent args)
    {
        if (!_fisherQuery.TryComp(args.User, out var fisherComp) || fisherComp.EndTime == null || args.Handled == true || !_timing.IsFirstTimePredicted)
            return;

        fisherComp.EndTime -= TimeSpan.FromSeconds(fisherComp.ProgressPerUse * component.Efficiency);
        args.Handled = true;
    }

    private void OnThrowFloat(EntityUid uid, FishingRodComponent component, ThrowFishingLureActionEvent args)
    {
        if (args.Handled || !_timing.IsFirstTimePredicted)
            return;

        var player = args.Performer;

        if (component.FishingLure != null)
        {
            _actions.RemoveAction(component.ThrowLureActionEntity);
            _actions.AddAction(player, ref component.PullLureActionEntity, component.PullLureActionId, uid);
            args.Handled = true;
            return;
        }

        if (_net.IsServer) // because i hate prediction
        {
            var targetCoords = _transform.ToMapCoordinates(args.Target);
            var playerCoords = _transform.GetMapCoordinates(Transform(player));

            var fishFloat = Spawn(component.FloatPrototype, playerCoords);
            component.FishingLure = fishFloat;
            Dirty(uid, component);

            // Calculate throw direction
            var direction = targetCoords.Position - playerCoords.Position;
            if (direction == Vector2.Zero)
                direction = Vector2.UnitX; // If the user somehow manages to click directly in the center of themself, just toss it to the right i guess.

            // Yeet
            _throwing.TryThrow(fishFloat, direction, 15f, player, 2f, null, true);

            // Set up lure component
            var fishLureComp = EnsureComp<FishingLureComponent>(fishFloat);
            fishLureComp.FishingRod = uid;

            // Rope visuals
            var visuals = EnsureComp<JointVisualsComponent>(fishFloat);
            visuals.Sprite = component.RopeSprite;
            visuals.OffsetA = new Vector2(0, 0.1f);
            visuals.OffsetB = component.RopeOffset;
            visuals.Target = GetNetEntity(uid);
        }

        _actions.RemoveAction(component.ThrowLureActionEntity);
        _actions.AddAction(player, ref component.PullLureActionEntity, component.PullLureActionId, uid);

        args.Handled = true;
    }

    private void OnPullFloat(EntityUid uid, FishingRodComponent component, PullFishingLureActionEvent args)
    {
        if (args.Handled || !_timing.IsFirstTimePredicted)
            return;

        var player = args.Performer;

        if (component.FishingLure == null)
        {
            _actions.RemoveAction(component.PullLureActionEntity);
            _actions.AddAction(player, ref component.ThrowLureActionEntity, component.ThrowLureActionId, uid);
            args.Handled = true;
            return;
        }

        _popup.PopupEntity(Loc.GetString("fishing-rod-remove-lure", ("ent", Name(uid))), uid);

        if (TryComp<FishingLureComponent>(component.FishingLure, out var lureComp) && lureComp.AttachedEntity != null && Exists(lureComp.AttachedEntity))
        {
            // TODO: so this kinda just lets you pull anything right up to you, it should instead just apply an impulse in your direction modfiied by the weight of the player vs the object
            // Also we need to autoreel/snap the line if the player gets too far away
            // Also we should probably PVS override the lure if the rod is in PVS, and vice versa to stop the joint visuals from popping in/out
            var attachedEnt = lureComp.AttachedEntity.Value;
            var targetCoords = _transform.GetMapCoordinates(Transform(attachedEnt));
            var playerCoords = _transform.GetMapCoordinates(Transform(player));
            var rand = new System.Random((int) _timing.CurTick.Value); // evil random prediction hack

            // Calculate throw direction
            var direction = (playerCoords.Position - targetCoords.Position) * rand.NextFloat(0.2f, 0.85f);

            // Yeet
            _throwing.TryThrow(attachedEnt, direction, 4f, player, 2f);
        }

        QueueDel(component.FishingLure);
        component.FishingLure = null;
        RemComp<ActiveFisherComponent>(player);

        _actions.RemoveAction(component.PullLureActionEntity);
        _actions.AddAction(player, ref component.ThrowLureActionEntity, component.ThrowLureActionId, uid);

        args.Handled = true;
    }

    private void OnFloatCollide(EntityUid uid, FishingLureComponent component, ref StartCollideEvent args)
    {
        // TODO:  make it so this can collide with any unacnchored objects (items, mobs, etc) but not the player casting it (get parent of rod?)
        var attachedEnt = args.OtherEntity;
        component.AttachedEntity = attachedEnt;

        // Anchor fishing float on an entity
        var spotPosition = _transform.GetWorldPosition(attachedEnt);
        _transform.SetWorldPosition(uid, spotPosition);
        _transform.AnchorEntity(uid);

        // Fishing spot logic
        if (HasComp<ActiveFishingSpotComponent>(attachedEnt) || !_fishSpotQuery.TryComp(attachedEnt, out var spotComp))
            return;

        var rand = new System.Random((int) _timing.CurTick.Value); // evil random prediction hack

        var activeFishSpot = EnsureComp<ActiveFishingSpotComponent>(attachedEnt);
        activeFishSpot.FishDifficulty = spotComp.FishDifficulty + rand.NextFloat(-spotComp.FishDifficultyVariety, spotComp.FishDifficultyVariety);
        activeFishSpot.Accumulator = spotComp.FishDefaultTimer + rand.NextFloat(-spotComp.FishTimerVariety, spotComp.FishTimerVariety);
        activeFishSpot.FishList = spotComp.FishList;
        activeFishSpot.AttachedFishingLure = uid;
    }

    private void OnFishingRodInit(Entity<FishingRodComponent> ent, ref MapInitEvent args)
        => _actions.AddAction(ent, ref ent.Comp.ThrowLureActionEntity, ent.Comp.ThrowLureActionId);

    private void OnGetActions(Entity<FishingRodComponent> ent, ref GetItemActionsEvent args)
    {
        if (ent.Comp.FishingLure == null)
            args.AddAction(ref ent.Comp.ThrowLureActionEntity, ent.Comp.ThrowLureActionId);
        else
            args.AddAction(ref ent.Comp.PullLureActionEntity, ent.Comp.PullLureActionId);
    }
}
