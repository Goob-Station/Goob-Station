using Content.Goobstation.Shared.Fishing.Components;
using Content.Goobstation.Shared.Fishing.Events;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Throwing;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Content.Shared.Actions;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Fishing.Systems;

/// <summary>
/// This handles... da fish and fishing process itself
/// </summary>
public abstract class SharedFishingSystem : EntitySystem
{
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] protected readonly ThrowingSystem Throwing = default!;
    [Dependency] protected readonly SharedTransformSystem Xform = default!;
    [Dependency] protected readonly SharedActionsSystem Actions = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    protected EntityQuery<ActiveFisherComponent> FisherQuery;
    protected EntityQuery<ActiveFishingSpotComponent> ActiveFishSpotQuery;
    protected EntityQuery<FishingSpotComponent> FishSpotQuery;
    protected EntityQuery<FishingRodComponent> FishRodQuery;
    protected EntityQuery<FishingLureComponent> FishLureQuery;

    public override void Initialize()
    {
        base.Initialize();

        FisherQuery = GetEntityQuery<ActiveFisherComponent>();
        ActiveFishSpotQuery = GetEntityQuery<ActiveFishingSpotComponent>();
        FishSpotQuery = GetEntityQuery<FishingSpotComponent>();
        FishRodQuery = GetEntityQuery<FishingRodComponent>();
        FishLureQuery = GetEntityQuery<FishingLureComponent>();

        SubscribeLocalEvent<FishingRodComponent, MapInitEvent>(OnFishingRodInit);
        SubscribeLocalEvent<FishingRodComponent, GetItemActionsEvent>(OnGetActions);
        SubscribeLocalEvent<FishingRodComponent, ThrowFishingLureActionEvent>(OnThrowFloat);
        SubscribeLocalEvent<FishingRodComponent, PullFishingLureActionEvent>(OnPullFloat);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        UpdateFishing();
    }

    private void UpdateFishing()
    {
        if (!Timing.IsFirstTimePredicted)
            return;

        var currentTime = Timing.CurTime;
        var activeFishers = EntityQueryEnumerator<ActiveFisherComponent>();
        while (activeFishers.MoveNext(out var fisher, out var fisherComp))
        {
            // Get fishing rod, then float, then spot... ReCurse.
            if (!FishRodQuery.TryComp(fisherComp.FishingRod, out var fishingRodComp))
                continue;
            if (!FishLureQuery.TryComp(fishingRodComp.FishingLure, out var fishingFloatComp))
                continue;
            if (!ActiveFishSpotQuery.TryComp(fishingFloatComp.AttachedEntity, out var activeSpotComp))
                continue;

            var fishRod = fisherComp.FishingRod;
            var fishSpot = fishingFloatComp.AttachedEntity.Value;

            if (!_hands.IsHolding(fisher, fishRod))
            {
                _popup.PopupEntity(Loc.GetString("fishing-progress-lost-rod", ("ent", Name(fishRod))), fisher, fisher);
                StopFishing((fishRod, fishingRodComp), fisher, fishSpot);
                continue;
            }

            fisherComp.TotalProgress ??= 0.5f;
            fisherComp.NextStruggle ??= Timing.CurTime + TimeSpan.FromSeconds(0.5f);

            // Fish fighting logic
            CalculateFightingTimings((fisher ,fisherComp), activeSpotComp);

            if (fisherComp.TotalProgress < 0f)
            {
                // It's over
                _popup.PopupEntity(Loc.GetString("fishing-progress-fail"), fisher, fisher);
                StopFishing((fishRod, fishingRodComp), fisher, fishSpot);
                continue;
            }

            if (fisherComp.TotalProgress >= 1.0f)
            {
                if (activeSpotComp.Fish != null)
                {
                    ThrowFishReward(activeSpotComp.Fish.Value, fishSpot, fisher);
                    _popup.PopupEntity(Loc.GetString("fishing-progress-success"), fisher, fisher);
                }

                StopFishing((fishRod, fishingRodComp), fisher, fishSpot);
            }
        }

        var fishingSpots = EntityQueryEnumerator<ActiveFishingSpotComponent>();
        while (fishingSpots.MoveNext(out var activeSpotComp))
        {
            if (currentTime < activeSpotComp.FishingStartTime || activeSpotComp.IsActive || activeSpotComp.FishingStartTime == null)
                continue;

            // Trigger start of the fishing process
            if (TerminatingOrDeleted(activeSpotComp.AttachedFishingLure))
                continue;

            // Get fishing lure, then rod, then player... ReCurse.
            if (!FishLureQuery.TryComp(activeSpotComp.AttachedFishingLure, out var fishingFloatComp) ||
                !FishRodQuery.TryComp(fishingFloatComp.FishingRod, out var fishRodComp))
                continue;

            var fishRod = fishingFloatComp.FishingRod;
            var fisher = Transform(fishingFloatComp.FishingRod).ParentUid;

            if (!_hands.IsHolding(fisher, fishRod) ||
                !HasComp<ActorComponent>(fisher))
                continue;

            var activeFisher = EnsureComp<ActiveFisherComponent>(fisher);
            activeFisher.FishingRod = fishRod;
            activeFisher.ProgressPerUse *= fishRodComp.Efficiency;
            activeFisher.TotalProgress = fishRodComp.StartingProgress;
            activeFisher.NextStruggle = Timing.CurTime + TimeSpan.FromSeconds(0.3f); // Compensate ping for 0.3 seconds

            _popup.PopupEntity(Loc.GetString("fishing-progress-start"), fisher, fisher);
            activeSpotComp.IsActive = true;
        }

        var fishingLures = EntityQueryEnumerator<FishingLureComponent>();
        while (fishingLures.MoveNext(out var fishingLure, out var lureComp))
        {
            if (lureComp.NextUpdate > Timing.CurTime)
                continue;

            lureComp.NextUpdate = Timing.CurTime + TimeSpan.FromSeconds(lureComp.UpdateInterval);

            if (!FishRodQuery.TryComp(lureComp.FishingRod, out var fishingRodComp))
                continue;

            var lurePos = Xform.GetMapCoordinates(fishingLure);
            var rodPos = Xform.GetMapCoordinates(lureComp.FishingRod);
            var distance = lurePos.Position - rodPos.Position;
            var fisher = Transform(lureComp.FishingRod).ParentUid;

            if (distance.Length() > fishingRodComp.BreakOnDistance ||
                lurePos.MapId != rodPos.MapId ||
                !_hands.IsHolding(fisher, lureComp.FishingRod) ||
                !HasComp<ActorComponent>(fisher))
            {
                var rod = (lureComp.FishingRod, fishingRodComp);
                StopFishing(rod, fisher, lureComp.AttachedEntity);
                ToggleFishingActions(rod, fisher, false);
            }
        }
    }

    /// <summary>
    /// if AddPulling is true, we ADD Pulling action and REMOVE Throwing action.
    /// Basically true if we start, and false if we end.
    /// </summary>
    protected void ToggleFishingActions(Entity<FishingRodComponent> ent, EntityUid fisher, bool addPulling)
    {
        if (addPulling)
        {
            Actions.RemoveAction(ent.Comp.ThrowLureActionEntity);
            Actions.AddAction(fisher, ref ent.Comp.PullLureActionEntity, ent.Comp.PullLureActionId, ent);
        }
        else
        {
            Actions.RemoveAction(ent.Comp.PullLureActionEntity);
            Actions.AddAction(fisher, ref ent.Comp.ThrowLureActionEntity, ent.Comp.ThrowLureActionId, ent);
        }
    }

    protected abstract void CalculateFightingTimings(Entity<ActiveFisherComponent> fisher, ActiveFishingSpotComponent activeSpotComp);

    private void OnThrowFloat(Entity<FishingRodComponent> ent, ref ThrowFishingLureActionEvent args)
    {
        if (args.Handled || !Timing.IsFirstTimePredicted)
            return;

        var player = args.Performer;

        if (ent.Comp.FishingLure != null || !Xform.IsValid(args.Target))
        {
            args.Handled = true;
            return;
        }

        SetupFishingFloat(ent, player, args.Target);
        ToggleFishingActions(ent, player, true);
        args.Handled = true;
    }

    /// <summary>
    /// Server-side only, sets up fishing rod
    /// </summary>
    protected abstract void SetupFishingFloat(Entity<FishingRodComponent> fishingRod, EntityUid player, EntityCoordinates target);

    /// <summary>
    /// Server-side only, spawns a fish and throws it to our player!
    /// </summary>
    protected abstract void ThrowFishReward(EntProtoId fishId, EntityUid fishSpot, EntityUid target);

    private void OnPullFloat(Entity<FishingRodComponent> ent, ref PullFishingLureActionEvent args)
    {
        if (args.Handled || !Timing.IsFirstTimePredicted)
            return;

        var player = args.Performer;
        var (uid, component) = ent;

        if (component.FishingLure == null)
        {
            ToggleFishingActions(ent, player, true);
            args.Handled = true;
            return;
        }

        _popup.PopupPredicted(Loc.GetString("fishing-rod-remove-lure", ("ent", Name(uid))), uid, uid);

        if (!FishLureQuery.TryComp(component.FishingLure, out var lureComp))
            return;

        if (lureComp.AttachedEntity != null && Exists(lureComp.AttachedEntity))
        {
            // TODO: so this kinda just lets you pull anything right up to you, it should instead just apply an impulse in your direction modfiied by the weight of the player vs the object
            // Also we need to autoreel/snap the line if the player gets too far away
            // Also we should probably PVS override the lure if the rod is in PVS, and vice versa to stop the joint visuals from popping in/out
            var attachedEnt = lureComp.AttachedEntity.Value;
            var targetCoords = Xform.GetMapCoordinates(Transform(attachedEnt));
            var playerCoords = Xform.GetMapCoordinates(Transform(player));
            var rand = new Random((int) Timing.CurTick.Value); // evil random prediction hack

            // Calculate throw direction
            var direction = (playerCoords.Position - targetCoords.Position) * rand.NextFloat(0.2f, 0.85f);

            // Yeet
            Throwing.TryThrow(attachedEnt, direction, 4f, player);
        }

        StopFishing(ent, player, lureComp.AttachedEntity);
        ToggleFishingActions(ent, player, false);
        args.Handled = true;
    }

    /// <summary>
    /// Reels the fishing rod back and stops fishing progress if arguments are passed to it.
    /// Server also deletes Fishing Lure
    /// </summary>
    protected virtual void StopFishing(
        Entity<FishingRodComponent> fishingRod,
        EntityUid? fisher,
        EntityUid? attachedEnt)
    {
        if (fishingRod.Comp.FishingLure == null)
            return;

        var lureComp = FishLureQuery.Comp(fishingRod.Comp.FishingLure.Value);
        ActiveFishSpotQuery.TryComp(lureComp.AttachedEntity, out var activeSpotComp);
        FisherQuery.TryComp(fisher, out var fisherComp);

        if (attachedEnt != null && activeSpotComp != null)
            RemCompDeferred(attachedEnt.Value, activeSpotComp);
        if (fisher != null && fisherComp != null)
        {
            RemCompDeferred(fisher.Value, fisherComp);
            ToggleFishingActions(fishingRod, fisher.Value, false);
        }
        fishingRod.Comp.FishingLure = null;
    }

    private void OnFishingRodInit(Entity<FishingRodComponent> ent, ref MapInitEvent args)
        => Actions.AddAction(ent, ref ent.Comp.ThrowLureActionEntity, ent.Comp.ThrowLureActionId);

    private void OnGetActions(Entity<FishingRodComponent> ent, ref GetItemActionsEvent args)
    {
        if (ent.Comp.FishingLure == null)
            args.AddAction(ref ent.Comp.ThrowLureActionEntity, ent.Comp.ThrowLureActionId);
        else
            args.AddAction(ref ent.Comp.PullLureActionEntity, ent.Comp.PullLureActionId);
    }
}
