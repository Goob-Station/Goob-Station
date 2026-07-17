// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Shared.Teleportation.Systems;
using Content.Shared.Destructible.Thresholds;
using Content.Shared.EntityEffects;
using Content.Shared.Examine;
using Content.Shared.Mobs.Components;
using Content.Shared.Tag;
using Content.Shared.Chemistry.Reaction;
using Robust.Shared.Prototypes;

// server cause i swear i saw this break some shit  at some point but i cant be bothered to replicate
namespace Content.Goobstation.Server.EntityEffects;

public sealed partial class RandomTeleportNearbySystem : EntityEffectSystem<ReactiveComponent, RandomTeleportNearby>
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly ExamineSystemShared _occlusion = default!;
    [Dependency] private readonly SharedRandomTeleportSystem _teleport = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    protected override void Effect(Entity<ReactiveComponent> entity, ref EntityEffectEvent<RandomTeleportNearby> args)
    {
        var uid = entity.Owner;
        var xform = _transform.GetMapCoordinates(uid);

        var entities = _lookup.GetEntitiesInRange<MobStateComponent>(xform, args.Effect.Range);

        if (entities.Count == 0)
            return;

        //Prevent Positronic Brain to get teleported too
        entities.RemoveWhere(ent => //todo upstreamtest
            TryComp<TagComponent>(ent, out var tagComp) &&
            _tag.HasTag(tagComp, "Brain"));

        var range = args.Effect.Range;

        var canTarget = entities
            .Where(target => _occlusion.InRangeUnOccluded(uid, target, range))
            .ToHashSet();

        if (canTarget.Count == 0)
            return;

        foreach (var target in canTarget)
        {
            _teleport.RandomTeleport(target, args.Effect.Radius, args.Effect.TeleportAttempts);
        }
    }
}

public sealed partial class RandomTeleportNearby : EntityEffectBase<RandomTeleportNearby>
{
    [DataField]
    public float Range = 7;

    /// <summary>
    ///     Up to how far to teleport the user in tiles.
    /// </summary>
    [DataField]
    public MinMax Radius = new MinMax(5, 20);

    /// <summary>
    ///     How many times to try to pick the destination. Larger number means the teleport is more likely to be safe.
    /// </summary>
    [DataField]
    public int TeleportAttempts = 10;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null;
}
