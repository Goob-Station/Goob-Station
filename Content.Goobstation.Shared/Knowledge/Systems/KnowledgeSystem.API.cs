using System.Diagnostics.CodeAnalysis;
using Content.Goobstation.Common.Knowledge.Components;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Knowledge.Systems;

public sealed partial class KnowledgeSystem
{
    [PublicAPI]
    public bool EnsureKnowledgeUnit(
        EntityUid ent,
        EntProtoId knowledgeId,
        [NotNullWhen(true)] out EntityUid? found)
    {
        var container = EnsureComp<KnowledgeContainerComponent>(ent);

        if (TryGetKnowledgeUnit((ent, container), knowledgeId, out found))
            return true;

        return PredictedTrySpawnInContainer(knowledgeId, ent, KnowledgeContainerComponent.ContainerId, out found);
    }

    [PublicAPI]
    public void EnsureKnowledgeUnits(
        EntityUid ent,
        List<EntProtoId> knowledgeIds,
        out List<EntityUid> foundList)
    {
        foundList = new List<EntityUid>();
        var container = EnsureComp<KnowledgeContainerComponent>(ent);

        foreach (var knowledgeId in knowledgeIds)
        {
            if (TryGetKnowledgeUnit((ent, container), knowledgeId, out var found))
            {
                foundList.Add(found.Value);
                continue;
            }

            if (PredictedTrySpawnInContainer(knowledgeId, ent, KnowledgeContainerComponent.ContainerId, out found))
                foundList.Add(found.Value);
        }
    }

    [PublicAPI]
    public bool TryEnsureKnowledgeUnit(
        Entity<KnowledgeContainerComponent?> ent,
        EntProtoId knowledgeId,
        [NotNullWhen(true)] out EntityUid? found)
    {
        found = null;
        if (!_containerQuery.Resolve(ent.Owner, ref ent.Comp))
            return false;

        if (TryGetKnowledgeUnit(ent, knowledgeId, out found))
            return true;

        return PredictedTrySpawnInContainer(knowledgeId, ent.Owner, KnowledgeContainerComponent.ContainerId, out found);
    }

    [PublicAPI]
    public bool TryAddKnowledgeUnit(Entity<KnowledgeContainerComponent?> ent, EntProtoId knowledgeId)
    {
        if (!_containerQuery.Resolve(ent.Owner, ref ent.Comp))
            return false;

        if (HasKnowledgeUnit(ent, knowledgeId))
            return false;

        PredictedTrySpawnInContainer(knowledgeId, ent.Owner, KnowledgeContainerComponent.ContainerId, out _);
        return true;
    }

    [PublicAPI]
    public bool RemoveKnowledgeUnit(Entity<KnowledgeContainerComponent?> ent, EntProtoId knowledgeUnit)
    {
        if (!_containerQuery.Resolve(ent.Owner, ref ent.Comp))
            return false;

        if (!TryGetKnowledgeUnit(ent, knowledgeUnit, out var unit))
            return false;

        PredictedQueueDel(unit);
        return true;
    }

    [PublicAPI]
    public bool TryGetKnowledgeUnit(Entity<KnowledgeContainerComponent?> ent, EntProtoId knowledgeUnit, [NotNullWhen(true)] out EntityUid? found)
    {
        found = null;
        if (!_containerQuery.Resolve(ent.Owner, ref ent.Comp))
            return false;

        foreach (var unit in ent.Comp.KnowledgeContainer?.ContainedEntities ?? [])
        {
            var prototype = Prototype(unit)?.ID;
            if (prototype is null
                || prototype != knowledgeUnit)
                continue;

            found = unit;
            break;
        }

        return found != null;
    }

    [PublicAPI]
    public bool HasKnowledgeUnit(Entity<KnowledgeContainerComponent?> ent, EntProtoId knowledgeUnit)
    {
        if (!_containerQuery.Resolve(ent.Owner, ref ent.Comp))
            return false;

        foreach (var unit in ent.Comp.KnowledgeContainer?.ContainedEntities ?? [])
        {
            var prototype = Prototype(unit)?.ID;
            if (prototype is null
                || prototype != knowledgeUnit)
                continue;

            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if the specified component is present on any of the entity's knowledge.
    /// </summary>
    [PublicAPI]
    public bool HasKnowledgeComp<T>(EntityUid? target) where T : IComponent
    {
        if (!_containerQuery.TryComp(target, out var container))
            return false;

        foreach (var effect in container.KnowledgeContainer?.ContainedEntities ?? [])
        {
            if (HasComp<T>(effect))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Returns all knowledge that have the specified component.
    /// </summary>
    [PublicAPI]
    public bool TryKnowledgeWithComp<T>(EntityUid? target, [NotNullWhen(true)] out HashSet<Entity<T, KnowledgeComponent>>? effects) where T : IComponent
    {
        effects = null;
        if (!_containerQuery.TryComp(target, out var container))
            return false;

        foreach (var effect in container.KnowledgeContainer?.ContainedEntities ?? [])
        {
            if (!_knowledgeQuery.TryComp(effect, out var statusComp))
                continue;

            if (TryComp<T>(effect, out var comp))
            {
                effects ??= [];
                effects.Add((effect, comp, statusComp));
            }
        }

        return effects is not null;
    }
}
