using System.Linq;
using Content.Shared._Goobstation.Fishing.Components;
using Content.Shared._Goobstation.Fishing.Systems;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Events;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Goobstation.Fishing;

public sealed class FishingSystem : SharedFishingSystem
{
    // Here we calculate the start of fishing, because apparently StartCollideEvent
    // works janky on clientside so we can't predict when fishing starts.
    [Dependency] private readonly IComponentFactory _compFactory = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FishingLureComponent, StartCollideEvent>(OnFloatCollide);
    }

    private void OnFloatCollide(EntityUid uid, FishingLureComponent component, ref StartCollideEvent args)
    {
        // TODO:  make it so this can collide with any unacnchored objects (items, mobs, etc) but not the player casting it (get parent of rod?)
        // Fishing spot logic
        var attachedEnt = args.OtherEntity;

        if (HasComp<ActiveFishingSpotComponent>(attachedEnt))
            return;

        var spotPosition = Xform.GetWorldPosition(attachedEnt);
        if (!FishSpotQuery.TryComp(attachedEnt, out var spotComp))
        {
            if (args.OtherBody.BodyType != BodyType.Dynamic)
                return;

            // Anchor fishing float on an entity
            Xform.SetWorldPosition(uid, spotPosition);
            Xform.AnchorEntity(uid);
            return;
        }

        // Anchor fishing float on an entity
        Xform.SetWorldPosition(uid, spotPosition);
        Xform.AnchorEntity(uid);
        component.AttachedEntity = attachedEnt;

        // Currently we don't support multiple loots from this
        var fish = spotComp.FishList.GetSpawns(_random.GetRandom(), EntityManager, _proto).First();

        // Get fish difficulty
        _proto.Index(fish).TryGetComponent(out FishComponent? fishComp, _compFactory);
        var difficulty = fishComp?.FishDifficulty ?? FishComponent.DefaultDifficulty;
        var variety = fishComp?.FishDifficultyVarirty ?? FishComponent.DefaultDifficultyVariety;

        // Assign things that depend on the fish
        var activeFishSpot = EnsureComp<ActiveFishingSpotComponent>(attachedEnt);
        activeFishSpot.Fish = fish;
        activeFishSpot.FishDifficulty = difficulty + rand.NextFloat(-variety, variety);

        // Assign things that depend on the spot
        var time = spotComp.FishDefaultTimer + rand.NextFloat(-spotComp.FishTimerVariety, spotComp.FishTimerVariety);
        activeFishSpot.FishingStartTime = Timing.CurTime + TimeSpan.FromSeconds(time);
        activeFishSpot.AttachedFishingLure = uid;

        // Declares war on prediction
        Dirty(attachedEnt, activeFishSpot);
        Dirty(uid, component);
    }
}
