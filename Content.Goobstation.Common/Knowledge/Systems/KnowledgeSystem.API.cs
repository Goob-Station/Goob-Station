using System.Diagnostics.CodeAnalysis;
using Content.Goobstation.Common.Knowledge.Components;
using Content.Goobstation.Common.Knowledge.Prototypes;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Common.Knowledge.Systems;

public sealed partial class KnowledgeSystem
{
    /// <summary>
    /// Ensures that knowledge unit exists inside an entity, and adds it if it's not already here.
    /// </summary>
    /// <returns>
    /// False if target entity has no <see cref="KnowledgeContainerComponent"/> or failed to spawn a knowledge unit inside it,
    /// true if unit was found or spawned successfully.
    /// </returns>
    public bool TryEnsureKnowledgeUnit(
        EntityUid target,
        EntProtoId knowledgeId,
        [NotNullWhen(true)] out EntityUid? found)
    {
        found = null;
        EnsureKnowledgeContainer(target, out var ent);
        EnsureContainer(ent);

        if (TryGetKnowledgeUnit(ent.Owner, knowledgeId, out found))
            return true;

        return PredictedTrySpawnInContainer(knowledgeId, ent.Owner, KnowledgeContainerComponent.ContainerId, out found);
    }

    /// <summary>
    /// Adds a knowledge unit to a knowledge container.
    /// </summary>
    /// <returns>
    /// False if target entity has no <see cref="KnowledgeContainerComponent"/>, or if container already has knowledge entity with that ID.
    /// </returns>
    public bool TryAddKnowledgeUnit(EntityUid target, EntProtoId knowledgeId)
    {
        EnsureKnowledgeContainer(target, out var ent);
        EnsureContainer(ent);

        if (HasKnowledgeUnit(ent.Owner, knowledgeId))
            return false;

        PredictedTrySpawnInContainer(knowledgeId, ent.Owner, KnowledgeContainerComponent.ContainerId, out _);
        return true;
    }

    /// <summary>
    /// Removes a knowledge unit from a container. This version takes into account levels and categories of knowledge.
    /// If knowledge has higher level than specified in the method, or a different category, it will not be removed.
    /// </summary>
    /// <param name="target">Entity to remove a unit from.</param>
    /// <param name="knowledgeUnit">Knowledge unit to remove.</param>
    /// <param name="category">Category of knowledge that we are removing.</param>
    /// <param name="level">Level of removal, that will remove knowledge only if its level is lower or equal to that value.</param>
    /// <param name="force">If true, will override all checks and will just always remove this knowledge.</param>
    /// <returns>True if removed successfully.</returns>
    public bool TryRemoveKnowledgeUnit(
        EntityUid target,
        EntProtoId knowledgeUnit,
        ProtoId<KnowledgeCategoryPrototype> category,
        int level,
        bool force = false)
    {
        if (!TryGetKnowledgeUnit(target, knowledgeUnit, out var unit)
            || !_knowledgeQuery.TryComp(unit, out var knowledge)
            || !CanRemoveKnowledge(knowledge.MemoryLevel, category, level, force))
            return false;

        PredictedQueueDel(unit);
        return true;
    }

    /// <summary>
    /// Removes a knowledge unit from a container. Will not remove a knowledge unit if it's marked as unremoveable,
    /// unless force parameter is true.
    /// </summary>
    public bool TryRemoveKnowledgeUnit(EntityUid target, EntProtoId knowledgeUnit, bool force = false)
    {
        if (!TryGetKnowledgeUnit(target, knowledgeUnit, out var unit)
            || !_knowledgeQuery.TryComp(unit, out var knowledge))
            return false;

        var memoryProto = _protoMan.Index(knowledge.MemoryLevel);
        if (!force && memoryProto.Unremoveable)
            return false;

        PredictedQueueDel(unit);
        return true;
    }

    /// <summary>
    /// Same as TryRemoveKnowledgeUnit, but instead of removing one specific units, runs it on all knowledge units at once.
    /// </summary>
    /// <returns>
    /// False if the target is not a knowledge container.
    /// </returns>
    public bool TryRemoveAllKnowledgeUnits(EntityUid target, ProtoId<KnowledgeCategoryPrototype> category, int level, bool force = false)
    {
        if (!TryGetAllKnowledgeUnits(target, out var units))
            return false;

        foreach (var (unit, knowledgeComp) in units)
        {
            if (!CanRemoveKnowledge(knowledgeComp.MemoryLevel, category, level, force))
                continue;

            PredictedQueueDel(unit);
        }

        return true;
    }

    /// <summary>
    /// Same as TryRemoveKnowledgeUnit, but instead of removing one specific units, runs it on all knowledge units at once.
    /// </summary>
    /// <returns>
    /// False if the target is not a knowledge container.
    /// </returns>
    public bool TryRemoveAllKnowledgeUnits(EntityUid target, bool force = false)
    {
        if (!TryGetAllKnowledgeUnits(target, out var units))
            return false;

        foreach (var (unit, knowledgeComp) in units)
        {
            var memoryProto = _protoMan.Index(knowledgeComp.MemoryLevel);
            if (!force && memoryProto.Unremoveable)
                continue;

            PredictedQueueDel(unit);
        }

        return true;
    }

    /// <summary>
    /// Gets a knowledge unit based on its entity prototype ID.
    /// </summary>
    /// <returns>
    /// False if the target is not a knowledge container, or if knowledge unit wasn't found.
    /// </returns>
    public bool TryGetKnowledgeUnit(EntityUid target, EntProtoId knowledgeUnit, [NotNullWhen(true)] out EntityUid? found)
    {
        found = null;
        EnsureKnowledgeContainer(target, out var ent);
        EnsureContainer(ent, out var container);

        foreach (var unit in container.ContainedEntities)
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

    /// <summary>
    /// Checks if that knowledge unit already exists inside a knowledge container.
    /// </summary>
    /// <returns>
    /// False if the target is not a knowledge container, and true if knowledge unit with that ID was found.
    /// </returns>
    public bool HasKnowledgeUnit(EntityUid target, EntProtoId knowledgeUnit)
    {
        EnsureKnowledgeContainer(target, out var ent);
        EnsureContainer(ent, out var container);

        foreach (var unit in container.ContainedEntities)
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
    /// Returns all knowledge units inside the container component.
    /// </summary>
    public bool TryGetAllKnowledgeUnits(EntityUid target, [NotNullWhen(true)] out HashSet<Entity<KnowledgeComponent>>? found)
    {
        found = null;
        EnsureKnowledgeContainer(target, out var ent);
        EnsureContainer(ent, out var container);

        foreach (var unit in container.ContainedEntities)
        {
            if (!_knowledgeQuery.TryComp(unit, out var knowledgeComp))
                continue;

            found ??= [];
            found.Add((unit, knowledgeComp));
        }

        return found is not null;
    }

    /// <summary>
    /// Checks if the specified component is present on any of the entity's knowledge.
    /// </summary>
    public bool HasKnowledgeComp<T>(EntityUid target) where T : IComponent
    {
        EnsureKnowledgeContainer(target, out var ent);
        EnsureContainer(ent, out var container);

        foreach (var knowledge in container.ContainedEntities)
        {
            if (HasComp<T>(knowledge))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Returns all knowledge that have the specified component.
    /// </summary>
    public bool TryGetKnowledgeWithComp<T>(EntityUid target, [NotNullWhen(true)] out HashSet<Entity<T, KnowledgeComponent>>? knowledgeEnts) where T : IComponent
    {
        knowledgeEnts = null;
        EnsureKnowledgeContainer(target, out var ent);
        EnsureContainer(ent, out var container);

        foreach (var knowledge in container.ContainedEntities)
        {
            if (!_knowledgeQuery.TryComp(knowledge, out var knowledgeComp))
                continue;

            if (TryComp<T>(knowledge, out var comp))
            {
                knowledgeEnts ??= [];
                knowledgeEnts.Add((knowledge, comp, knowledgeComp));
            }
        }

        return knowledgeEnts is not null;
    }

    /// <summary>
    /// Returns true if that knowledge can be removed, by taking
    /// into account its memory level and knowledge category.
    /// </summary>
    public bool CanRemoveKnowledge(
        ProtoId<KnowledgeMemoryPrototype> target,
        ProtoId<KnowledgeCategoryPrototype> category,
        int level,
        bool force = false)
    {
        if (force)
            return true;

        var memoryProto = _protoMan.Index(target);

        if (memoryProto.Unremoveable
            || memoryProto.Category != category
            || memoryProto.Level > level)
            return false;

        return true;
    }

    /// <summary>
    /// Gets a knowledge container from an entity.
    /// Since sometimes the entity itself is a knowledge container, and sometimes it's contained in the brain,
    /// we have to sometimes relay to the brain entity to get knowledge properly.
    /// </summary>
    /// <param name="uid">Main entity from which we are trying to get</param>
    /// <returns>Entity that contains knowledge related to original uid.</returns>
    public Entity<KnowledgeContainerComponent> EnsureKnowledgeContainer(EntityUid uid)
    {
        // Raise event on all children
        var ev = new KnowledgeContainerRelayEvent(uid);
        RecursiveRaiseRelayEvent(uid, ref ev);

        // Check entity that we have found
        if (_containerQuery.TryComp(ev.Found, out var knowledgeFound))
            return (ev.Found.Value, knowledgeFound);

        // If not found just five up
        var knowledge = EnsureComp<KnowledgeContainerComponent>(uid);
        return (uid, knowledge);
    }

    /// <inheritdoc cref="EnsureKnowledgeContainer(Robust.Shared.GameObjects.EntityUid)"/>
    public void EnsureKnowledgeContainer(EntityUid uid, out Entity<KnowledgeContainerComponent> container)
    {
        // Raise event on all children
        var ev = new KnowledgeContainerRelayEvent(uid);
        RecursiveRaiseRelayEvent(uid, ref ev);

        // Check entity that we have found
        if (_containerQuery.TryComp(ev.Found, out var knowledgeFound))
        {
            container = (ev.Found.Value, knowledgeFound);
            return;
        }

        // If not found just give up and ensure it on the entity itself
        var knowledge = EnsureComp<KnowledgeContainerComponent>(uid);
        container = (uid, knowledge);
    }

    private void RecursiveRaiseRelayEvent(EntityUid uid, ref KnowledgeContainerRelayEvent ev)
    {
        var enumerator = Transform(uid).ChildEnumerator;
        while (enumerator.MoveNext(out var child))
        {
            RaiseLocalEvent(child, ref ev);
            RecursiveRaiseRelayEvent(child, ref ev);
        }
    }

    private void EnsureContainer(Entity<KnowledgeContainerComponent> ent)
    {
        ent.Comp.KnowledgeContainer = _container.EnsureContainer<Container>(ent.Owner, KnowledgeContainerComponent.ContainerId);
        // We show the contents of the container to allow knowledge to have visible sprites. I mean, if you really need to show some big brains.
        ent.Comp.KnowledgeContainer.ShowContents = true;
    }

    private void EnsureContainer(Entity<KnowledgeContainerComponent> ent, out Container container)
    {
        container = _container.EnsureContainer<Container>(ent.Owner, KnowledgeContainerComponent.ContainerId);
        // We show the contents of the container to allow knowledge to have visible sprites. I mean, if you really need to show some big brains.
        container.ShowContents = true;

        ent.Comp.KnowledgeContainer = container;
    }
}
