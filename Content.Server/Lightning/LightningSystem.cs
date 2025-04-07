using System.Linq;
using Content.Server.Beam;
using Content.Server.Beam.Components;
using Content.Server.Lightning.Components;
using Content.Shared.Lightning;
using Content.Shared.Physics;
using Content.Shared.Tag;
using Robust.Server.GameObjects;
using Robust.Shared.Physics;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server.Lightning;

// TheShuEd:
//I've redesigned the lightning system to be more optimized.
//Previously, each lightning element, when it touched something, would try to branch into nearby entities.
//So if a lightning bolt was 20 entities long, each one would check its surroundings and have a chance to create additional lightning...
//which could lead to recursive creation of more and more lightning bolts and checks.

//I redesigned so that lightning branches can only be created from the point where the lightning struck, no more collide checks
//and the number of these branches is explicitly controlled in the new function.
public sealed class LightningSystem : SharedLightningSystem
{
    [Dependency] private readonly BeamSystem _beam = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly PhysicsSystem _physics = default!; // Goobstation
    [Dependency] private readonly TagSystem _tag = default!; // Goobstation

    private static readonly ProtoId<TagPrototype> BlockLightningTag = "BlockLightning";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LightningComponent, ComponentRemove>(OnRemove);
    }

    private void OnRemove(EntityUid uid, LightningComponent component, ComponentRemove args)
    {
        if (!TryComp<BeamComponent>(uid, out var lightningBeam) || !TryComp<BeamComponent>(lightningBeam.VirtualBeamController, out var beamController))
        {
            return;
        }

        beamController.CreatedBeams.Remove(uid);
    }

    /// <summary>
    /// Fires lightning from user to target
    /// </summary>
    /// <param name="user">Where the lightning fires from</param>
    /// <param name="target">Where the lightning fires to</param>
    /// <param name="lightningPrototype">The prototype for the lightning to be created</param>
    /// <param name="triggerLightningEvents">if the lightnings being fired should trigger lightning events.</param>
    /// <param name="beamAction">Goobstation. Action that is called on each beam entity.</param>
    /// <param name="accumulateIndex">Goobstation. Whether to accumulate BeamSystem.NextIndex.</param>
    public bool ShootLightning(EntityUid user, EntityUid target, string lightningPrototype = "Lightning", bool triggerLightningEvents = true, Action<EntityUid>? beamAction = null, bool accumulateIndex = true)
    {
        // Goobstation start. This is required for force walls to block lightning so that you can't stand inside them
        // and spam lightning spells.
        var userMapPos = _transform.GetMapCoordinates(user);
        var targetMapPos = _transform.GetMapCoordinates(target);

        var direction = targetMapPos.Position - userMapPos.Position;
        var length = direction.Length();
        if (length == 0f)
            return false;

        var ray = new CollisionRay(userMapPos.Position,
            direction.Normalized(),
            (int) CollisionGroup.Opaque);

        var blocker = _physics
            .IntersectRayWithPredicate(userMapPos.MapId,
                ray,
                length,
                x => x == user || !_tag.HasTag(x, BlockLightningTag))
            .FirstOrNull()
            ?.HitEntity;

        if (blocker != null)
            target = blocker.Value;
        // Goobstation end

        var spriteState = LightningRandomizer();
        if (!_beam.TryCreateBeam(user, target, lightningPrototype, spriteState, beamAction: beamAction, accumulateIndex: accumulateIndex)) // Goob edit
            return false;

        if (triggerLightningEvents) // we don't want certain prototypes to trigger lightning level events
        {
            var ev = new HitByLightningEvent(user, target);
            RaiseLocalEvent(target, ref ev);
        }

        return blocker == null; // Goobstation
    }


    /// <summary>
    /// Looks for objects with a LightningTarget component in the radius, prioritizes them, and hits the highest priority targets with lightning.
    /// </summary>
    /// <param name="user">Where the lightning fires from</param>
    /// <param name="range">Targets selection radius</param>
    /// <param name="boltCount">Number of lightning bolts</param>
    /// <param name="lightningPrototype">The prototype for the lightning to be created</param>
    /// <param name="arcDepth">how many times to recursively fire lightning bolts from the target points of the first shot.</param>
    /// <param name="triggerLightningEvents">if the lightnings being fired should trigger lightning events.</param>
    /// <param name="ignoredEntity">Goobstation. Don't arc to this entity.</param>
    /// <param name="beamAction">Goobstation. Action that is called on each beam entity.</param>
    public void ShootRandomLightnings(EntityUid user, float range, int boltCount, string lightningPrototype = "Lightning", int arcDepth = 0, bool triggerLightningEvents = true, EntityUid? ignoredEntity = null, Action<EntityUid>? beamAction = null) // Goob edit
    {
        //TODO: add support to different priority target tablem for different lightning types
        //TODO: Remove Hardcode LightningTargetComponent (this should be a parameter of the SharedLightningComponent)
        //TODO: This is still pretty bad for perf but better than before and at least it doesn't re-allocate
        // several hashsets every time

        var targets = _lookup.GetEntitiesInRange<LightningTargetComponent>(_transform.GetMapCoordinates(user), range).ToList();
        targets = targets.Where(x => x.Owner != ignoredEntity).ToList(); // Goobstation
        _random.Shuffle(targets);
        targets.Sort((x, y) => y.Comp.Priority.CompareTo(x.Comp.Priority));

        int shootedCount = 0;
        int count = -1;
        while(shootedCount < boltCount)
        {
            count++;

            if (count >= targets.Count) { break; }

            var curTarget = targets[count];
            if (!_random.Prob(curTarget.Comp.HitProbability)) //Chance to ignore target
                continue;

            if (!ShootLightning(user, targets[count].Owner, lightningPrototype, triggerLightningEvents, beamAction, false)) // Goob edit
            {
                shootedCount++;
                continue;
            }
            if (arcDepth - targets[count].Comp.LightningResistance > 0)
            {
                ShootRandomLightnings(targets[count].Owner, range, 1, lightningPrototype, arcDepth - targets[count].Comp.LightningResistance, triggerLightningEvents, ignoredEntity, beamAction); // Goob edit
            }
            shootedCount++;
        }

        _beam.AccumulateIndex(); // Goobstation
    }
}

/// <summary>
/// Raised directed on the target when an entity becomes the target of a lightning strike (not when touched)
/// </summary>
/// <param name="Source">The entity that created the lightning</param>
/// <param name="Target">The entity that was struck by lightning.</param>
[ByRefEvent]
public readonly record struct HitByLightningEvent(EntityUid Source, EntityUid Target);
