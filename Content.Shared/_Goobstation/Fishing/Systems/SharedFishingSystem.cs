using System.Numerics;
using Content.Shared._Goobstation.Fishing.Components;
using Content.Shared._Goobstation.Fishing.Events;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Throwing;
using Robust.Shared.Network;
using Robust.Shared.Physics.Events;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

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

        SubscribeLocalEvent<FishingRodComponent, InteractUsingEvent>(OnFishingInteract);
        SubscribeLocalEvent<FishingRodComponent, ThrowFishingLureActionEvent>(OnThrowFloat);

        SubscribeLocalEvent<FishingLureComponent, StartCollideEvent>(OnFloatCollide);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        UpdateFishing(frameTime);
    }

    protected void UpdateFishing(float frameTime)
    {
        var activeFishers = EntityQueryEnumerator<ActiveFisherComponent>();
        while (activeFishers.MoveNext(out var fisher, out var fisherComp))
        {
            fisherComp.TotalProgress -= fisherComp.ProgressWithdraw * frameTime;

            // Get fishing rod, then float, then spot... ReCurse.
            if (!_fishRodQuery.TryComp(fisherComp.FishingRod, out var fishingRodComp) ||
                !_fishFloatQuery.TryComp(fishingRodComp.FishingLure, out var fishingFloatComp) ||
                !_activeFishSpotQuery.TryComp(fishingFloatComp.FishingSpot, out var activeSpotComp))
                continue;

            var fishRod = fisherComp.FishingRod;
            var fishingLure = fishingRodComp.FishingLure.Value;
            var fishSpot = fishingFloatComp.FishingSpot.Value;

            if (!_hands.IsHolding(fisher, fishRod))
            {
                _popup.PopupEntity(Loc.GetString("fishing-progress-lost-rod", ("ent", Name(fishRod))), fisher, fisher);
                // Cleanup entities and their connections
                RemComp(fisher, fisherComp);
                RemComp(fishSpot, activeSpotComp);
                QueueDel(fishingLure);
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
                fishingRodComp.FishingLure = null;
                continue;
            }

            if (fisherComp.TotalProgress >= 1.0f)
            {
                // We're spawning the fish only on the server, because I don't want to deal with networking just for this
                if (_net.IsServer)
                {
                    var fishIds = activeSpotComp.FishList.GetSpawns(_random.GetRandom(), EntityManager, _proto);
                    var position = Transform(fishingFloatComp.FishingSpot.Value).Coordinates;
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
            activeFisher.ProgressWithdraw += activeSpotComp.FishDifficulty;
            activeFisher.ProgressPerUse *= fishRodComp.Efficiency;

            _popup.PopupEntity(Loc.GetString("fishing-progress-start"), fisher, fisher);
            activeSpotComp.IsActive = true;
        }
    }

    private void OnFishingInteract(EntityUid uid, FishingRodComponent component, InteractUsingEvent args)
    {
        if (!_fisherQuery.TryComp(args.User, out var fisherComp))
            return;

        fisherComp.TotalProgress += fisherComp.ProgressPerUse * component.Efficiency;
    }

    private void OnThrowFloat(EntityUid uid, FishingRodComponent component, ThrowFishingLureActionEvent args)
    {
        if (args.Handled || !_timing.IsFirstTimePredicted)
            return;

        if (component.FishingLure != null)
        {
            _popup.PopupEntity(Loc.GetString("fishing-rod-remove-lure", ("ent", Name(uid))), uid);
            QueueDel(component.FishingLure);
            component.FishingLure = null;
            args.Handled = true;
            return;
        }

        if (!_net.IsServer) // because i hate prediction
            return;

        var player = args.Performer;
        var targetCoords = _transform.ToMapCoordinates(args.Target);
        var playerCoords = _transform.GetMapCoordinates(Transform(player));

        var fishFloat = Spawn(component.FloatPrototype, playerCoords);
        component.FishingLure = fishFloat;

        // Calculate throw direction
        var direction = targetCoords.Position - playerCoords.Position;
        if (direction == Vector2.Zero)
            direction = Vector2.UnitX; // If the user somehow manages to click directly in the center of themself, just toss it to the right i guess.

        // Yeet
        _throwing.TryThrow(fishFloat, direction, 15f, player, 2f);

        // Set up lure component
        var fishLureComp = EnsureComp<FishingLureComponent>(fishFloat);
        fishLureComp.FishingRod = uid;

        // Rope visuals
        var visuals = EnsureComp<JointVisualsComponent>(fishFloat);
        visuals.Sprite = component.RopeSprite;
        visuals.OffsetA = new Vector2(0, 0.1f);
        visuals.OffsetB = component.RopeOffset;
        visuals.Target = GetNetEntity(uid);

        args.Handled = true;

    }

    private void OnFloatCollide(EntityUid uid, FishingLureComponent component, ref StartCollideEvent args)
    {
        if (!_fishSpotQuery.TryComp(args.OtherEntity, out var spotComp))
            return;

        var fishingSpot = args.OtherEntity;

        // Anchor fishing float on a fishing spot
        var spotPosition = _transform.GetWorldPosition(fishingSpot);
        _transform.SetWorldPosition(uid, spotPosition);
        _transform.AnchorEntity(uid);

        var rand = new System.Random((int) _timing.CurTick.Value); // evil random prediction hack

        if (HasComp<ActiveFishingSpotComponent>(fishingSpot))
            return;

        // Start it up
        var activeFishSpot = EnsureComp<ActiveFishingSpotComponent>(fishingSpot);
        activeFishSpot.FishDifficulty = spotComp.FishDifficulty + rand.NextFloat(-spotComp.FishDifficultyVariety, spotComp.FishDifficultyVariety);
        activeFishSpot.Accumulator = spotComp.FishDefaultTimer + rand.NextFloat(-spotComp.FishTimerVariety, spotComp.FishTimerVariety);
        activeFishSpot.FishList = spotComp.FishList;
        activeFishSpot.AttachedFishingLure = uid;
        component.FishingSpot = fishingSpot;
    }
}
