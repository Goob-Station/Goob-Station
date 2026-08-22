using System.Diagnostics.CodeAnalysis;
using Content.Goobstation.Common.Knowledge;
using Content.Goobstation.Common.Knowledge.Components;
using JetBrains.Annotations;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Knowledge.Systems;

public sealed partial class KnowledgeSystem
{
    [PublicAPI]
    public override bool TryEnsureKnowledgeUnit(
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

    [PublicAPI]
    public override bool TryAddKnowledgeUnit(EntityUid target, EntProtoId knowledgeId)
    {
        EnsureKnowledgeContainer(target, out var ent);
        EnsureContainer(ent);

        if (HasKnowledgeUnit(ent.Owner, knowledgeId))
            return false;

        return PredictedTrySpawnInContainer(knowledgeId, ent.Owner, KnowledgeContainerComponent.ContainerId, out _);
    }

    [PublicAPI]
    public override bool TryAddKnowledgeUnit(
        EntityUid target,
        EntProtoId knowledgeId,
        [NotNullWhen(true)] out EntityUid? found)
    {
        found = null;
        EnsureKnowledgeContainer(target, out var ent);
        EnsureContainer(ent);

        if (HasKnowledgeUnit(ent.Owner, knowledgeId))
            return false;

        return PredictedTrySpawnInContainer(knowledgeId, ent.Owner, KnowledgeContainerComponent.ContainerId, out found);
    }

    [PublicAPI]
    public override void AddKnowledgeUnits(EntityUid target, List<EntProtoId> knowledgeList)
    {
        EnsureKnowledgeContainer(target, out var ent);
        EnsureContainer(ent);

        foreach (var knowledgeId in knowledgeList)
        {
            if (HasKnowledgeUnit(ent.Owner, knowledgeId))
                continue;

            PredictedTrySpawnInContainer(knowledgeId, ent.Owner, KnowledgeContainerComponent.ContainerId, out _);
        }
    }

    [PublicAPI]
    public override bool TryRemoveKnowledgeUnit(
        EntityUid target,
        EntProtoId knowledgeUnit,
        int level,
        bool force = false)
    {
        if (!TryGetKnowledgeUnit(target, knowledgeUnit, out var unit)
            || !_knowledgeQuery.TryComp(unit, out var knowledge)
            || !CanRemoveKnowledge((unit.Value, knowledge), level, force))
            return false;

        PredictedQueueDel(unit);
        return true;
    }

    [PublicAPI]
    public override bool TryRemoveKnowledgeUnit(EntityUid target, EntProtoId knowledgeUnit, bool force = false)
    {
        if (!TryGetKnowledgeUnit(target, knowledgeUnit, out var unit)
            || !_knowledgeQuery.TryComp(unit, out var knowledge))
            return false;

        if (!force && knowledge.Unremoveable)
            return false;

        PredictedQueueDel(unit);
        return true;
    }

    [PublicAPI]
    public override bool TryRemoveAllKnowledgeUnits(EntityUid target, int level, bool force = false)
    {
        if (!TryGetAllKnowledgeUnits(target, out var units))
            return false;

        foreach (var unit in units)
        {
            if (!CanRemoveKnowledge(unit.AsNullable(), level, force))
                continue;

            PredictedQueueDel(unit.Owner);
        }

        return true;
    }

    [PublicAPI]
    public override bool TryRemoveAllKnowledgeUnits(EntityUid target, bool force = false)
    {
        if (!TryGetAllKnowledgeUnits(target, out var units))
            return false;

        foreach (var (unit, knowledgeComp) in units)
        {
            if (!force && knowledgeComp.Unremoveable)
                continue;

            PredictedQueueDel(unit);
        }

        return true;
    }

    [PublicAPI]
    public override bool TryGetKnowledgeUnit(
        EntityUid target,
        EntProtoId knowledgeUnit,
        [NotNullWhen(true)] out EntityUid? found)
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

    [PublicAPI]
    public override bool HasKnowledgeUnit(EntityUid target, EntProtoId knowledgeUnit)
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

    [PublicAPI]
    public override bool TryGetAllKnowledgeUnits(
        EntityUid target,
        [NotNullWhen(true)] out HashSet<Entity<KnowledgeComponent>>? found)
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

    [PublicAPI]
    public override bool HasKnowledgeComp<T>(EntityUid target)
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

    [PublicAPI]
    public override bool TryGetKnowledgeWithComp<T>(
        EntityUid target,
        [NotNullWhen(true)] out HashSet<Entity<T, KnowledgeComponent>>? knowledgeEnts)
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

    [PublicAPI]
    public override bool CanRemoveKnowledge(
        Entity<KnowledgeComponent?> target,
        int level,
        bool force = false)
    {
        if (!_knowledgeQuery.Resolve(target.Owner, ref target.Comp))
            return false;

        if (force)
            return true;

        if (target.Comp.Unremoveable
            || target.Comp.Level > level)
            return false;

        return true;
    }

    [PublicAPI]
    public override Entity<KnowledgeContainerComponent> EnsureKnowledgeContainer(EntityUid uid)
    {
        // Raise event on an entity. Stuff like BodySystem should give us a result
        var ev = new KnowledgeContainerRelayEvent(uid);
        RaiseLocalEvent(uid, ref ev);

        // Check entity that we have found
        if (_containerQuery.TryComp(ev.Found, out var knowledgeFound))
            return (ev.Found.Value, knowledgeFound);

        // If not found just five up
        var knowledge = EnsureComp<KnowledgeContainerComponent>(uid);
        return (uid, knowledge);
    }

    [PublicAPI]
    public override void EnsureKnowledgeContainer(EntityUid uid, out Entity<KnowledgeContainerComponent> container)
    {
        // Raise event on an entity. Stuff like BodySystem should give us a result
        var ev = new KnowledgeContainerRelayEvent(uid);
        RaiseLocalEvent(uid, ref ev);

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

    [PublicAPI]
    public override string? GetKnowledgeString(Entity<KnowledgeComponent> knowledge)
    {
        var ev = new KnowledgeGetStringEvent();
        RaiseLocalEvent(knowledge.Owner, ref ev);
        return ev.Description;
    }
}
