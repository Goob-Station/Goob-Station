// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Shared.Teleportation.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Destructible.Thresholds;
using Content.Shared.EntityEffects;
using Content.Shared.Examine;
using Content.Shared.Mobs.Components;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityEffects;

public sealed partial class RandomTeleportNearby : EntityEffect
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

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null;

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (args is not EntityEffectReagentArgs reagentArgs)
            return;

        var entityManager = args.EntityManager;
        var uid = args.TargetEntity;

        var transformSystem = entityManager.System<SharedTransformSystem>();
        var lookupSys = entityManager.System<EntityLookupSystem>();
        var occlusionSys = entityManager.System<ExamineSystemShared>();
        var teleportSystem = entityManager.System<SharedRandomTeleportSystem>();
        var tagSystem = entityManager.System<TagSystem>();

        var xform = transformSystem.GetMapCoordinates(uid);

        var entities = lookupSys.GetEntitiesInRange<MobStateComponent>(xform, Range);

        if (entities.Count == 0)
            return;

        //Prevent Positronic Brain to get teleported too
        entities.RemoveWhere(ent => //todo upstreamtest
            entityManager.TryGetComponent<TagComponent>(ent, out var tag) &&
            tagSystem.HasTag(tag, "Brain"));

        var canTarget = entities
            .Where(entity => entity != null && occlusionSys.InRangeUnOccluded(uid, entity, Range))
            .ToHashSet();

        if (canTarget.Count == 0)
            return;

        foreach (var entity in canTarget)
            teleportSystem.RandomTeleport(entity, Radius, TeleportAttempts);
    }
}
