using System.Diagnostics.CodeAnalysis;
using Content.Goobstation.Common.Knowledge.Components;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Common.Knowledge;

/// <summary>
/// Provides ways to control all knowledge related entities.
/// </summary>
public interface IKnowledgeSystem
{
    /// <summary>
    /// Ensures that knowledge unit exists inside an entity, and adds it if it's not already here.
    /// </summary>
    /// <returns>
    /// False if or failed to spawn a knowledge unit inside it, true if unit was found or spawned successfully.
    /// </returns>
    [PublicAPI]
    bool TryEnsureKnowledgeUnit(
        EntityUid target,
        EntProtoId knowledgeId,
        [NotNullWhen(true)] out EntityUid? found);

    /// <summary>
    /// Adds a knowledge unit to a knowledge container.
    /// </summary>
    /// <returns>
    /// False if container already has knowledge entity with that ID.
    /// </returns>
    [PublicAPI]
    bool TryAddKnowledgeUnit(EntityUid target, EntProtoId knowledgeId);

    /// <inheritdoc cref="TryAddKnowledgeUnit(Robust.Shared.GameObjects.EntityUid,Robust.Shared.Prototypes.EntProtoId)"/>
    [PublicAPI]
    bool TryAddKnowledgeUnit(
        EntityUid target,
        EntProtoId knowledgeId,
        [NotNullWhen(true)] out EntityUid? found);

    /// <summary>
    /// Adds a list of knowledge units to a knowledge container.
    /// </summary>
    [PublicAPI]
    void AddKnowledgeUnits(EntityUid target, List<EntProtoId> knowledgeList);

    /// <summary>
    /// Removes a knowledge unit from a container. This version takes into account levels and categories of knowledge.
    /// If knowledge has higher level than specified in the method, or a different category, it will not be removed.
    /// </summary>
    /// <param name="target">Entity to remove a unit from.</param>
    /// <param name="knowledgeUnit">Knowledge unit to remove.</param>
    /// <param name="level">Level of removal, that will remove knowledge only if its level is lower or equal to that value.</param>
    /// <param name="force">If true, will override all checks and will just always remove this knowledge.</param>
    /// <returns>True if removed successfully.</returns>
    [PublicAPI]
    bool TryRemoveKnowledgeUnit(
        EntityUid target,
        EntProtoId knowledgeUnit,
        int level,
        bool force = false);

    /// <summary>
    /// Removes a knowledge unit from a container. Will not remove a knowledge unit if it's marked as unremoveable,
    /// unless force parameter is true.
    /// </summary>
    [PublicAPI]
    bool TryRemoveKnowledgeUnit(EntityUid target, EntProtoId knowledgeUnit, bool force = false);

    /// <summary>
    /// Same as TryRemoveKnowledgeUnit, but instead of removing one specific units, runs it on all knowledge units at once.
    /// </summary>
    /// <returns>
    /// False if the target is not a knowledge container.
    /// </returns>
    [PublicAPI]
    bool TryRemoveAllKnowledgeUnits(EntityUid target, int level, bool force = false);

    /// <summary>
    /// Same as TryRemoveKnowledgeUnit, but instead of removing one specific units, runs it on all knowledge units at once.
    /// </summary>
    /// <returns>
    /// False if the target is not a knowledge container.
    /// </returns>
    [PublicAPI]
    bool TryRemoveAllKnowledgeUnits(EntityUid target, bool force = false);

    /// <summary>
    /// Gets a knowledge unit based on its entity prototype ID.
    /// </summary>
    /// <returns>
    /// False if the target is not a knowledge container, or if knowledge unit wasn't found.
    /// </returns>
    [PublicAPI]
    bool TryGetKnowledgeUnit(
        EntityUid target,
        EntProtoId knowledgeUnit,
        [NotNullWhen(true)] out EntityUid? found);

    /// <summary>
    /// Checks if that knowledge unit already exists inside a knowledge container.
    /// </summary>
    /// <returns>
    /// False if the target is not a knowledge container, and true if knowledge unit with that ID was found.
    /// </returns>
    [PublicAPI]
    bool HasKnowledgeUnit(EntityUid target, EntProtoId knowledgeUnit);

    /// <summary>
    /// Returns all knowledge units inside the container component.
    /// </summary>
    [PublicAPI]
    bool TryGetAllKnowledgeUnits(
        EntityUid target,
        [NotNullWhen(true)] out HashSet<Entity<KnowledgeComponent>>? found);

    /// <summary>
    /// Checks if the specified component is present on any of the entity's knowledge.
    /// </summary>
    [PublicAPI]
    bool HasKnowledgeComp<T>(EntityUid target) where T : IComponent;

    /// <summary>
    /// Returns all knowledge that have the specified component.
    /// </summary>
    [PublicAPI]
    bool TryGetKnowledgeWithComp<T>(
        EntityUid target,
        [NotNullWhen(true)] out HashSet<Entity<T, KnowledgeComponent>>? knowledgeEnts)
        where T : IComponent;

    /// <summary>
    /// Returns true if that knowledge can be removed, by taking
    /// into account its memory level and knowledge category.
    /// </summary>
    [PublicAPI]
    bool CanRemoveKnowledge(
        Entity<KnowledgeComponent?> target,
        int level,
        bool force = false);

    /// <summary>
    /// Gets a knowledge container from an entity.
    /// Since sometimes the entity itself is a knowledge container, and sometimes it's contained in the brain,
    /// we have to sometimes relay to the brain entity to get knowledge properly.
    /// </summary>
    /// <param name="uid">Main entity from which we are trying to get</param>
    /// <returns>Entity that contains knowledge related to original uid.</returns>
    [PublicAPI]
    Entity<KnowledgeContainerComponent> EnsureKnowledgeContainer(EntityUid uid);

    /// <inheritdoc cref="EnsureKnowledgeContainer(Robust.Shared.GameObjects.EntityUid)"/>
    [PublicAPI]
    void EnsureKnowledgeContainer(EntityUid uid, out Entity<KnowledgeContainerComponent> container);

    [PublicAPI]
    string? GetKnowledgeString(Entity<KnowledgeComponent> knowledge);
}

