using System.Linq;
using Content.Shared.Tag;
using Robust.Shared.Containers;

// :)
namespace Moonyware.Miscellaneous.Systems;

/// <summary>
/// Container Query API for convenient checking for items by comp or tag on the entire entity
/// </summary>
/// <remarks>This checks <b>every</b> container on the entity, including organ ones!</remarks>
public sealed class StorageLookupSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tag = default!;

    private EntityQuery<ContainerManagerComponent> _containerQuery;

    public override void Initialize()
    {
        base.Initialize();

        _containerQuery = GetEntityQuery<ContainerManagerComponent>();
    }

    private EntityUid[] FindStoredInternal(EntityUid entity, Func<EntityUid, bool> predicate, bool firstMatchReturn = false)
    {
        if (!_containerQuery.TryGetComponent(entity, out ContainerManagerComponent? currentManager))
            return [];

        HashSet<EntityUid> entities = [];
        Stack<ContainerManagerComponent> stack = new();

        do
        {
            foreach (var container in currentManager.Containers.Values)
            {
                foreach (var ent in container.ContainedEntities)
                {
                    // check if this is the item
                    if (predicate(ent))
                    {
                        if (firstMatchReturn)
                            return [ent];

                        entities.Add(ent);
                    }

                    // if it is a container check its contents
                    if (_containerQuery.TryGetComponent(ent, out var containerManager))
                        stack.Push(containerManager);
                }
            }
        } while (stack.TryPop(out currentManager));

        return [.. entities];
    }

    #region By parent prototype

    public EntityUid[] FindFirstStoredByParent(EntityUid entity, string parentproto)
    {
        return FindStoredByParent(entity, parentproto, firstMatchReturn: true);
    }

    public EntityUid[] FindAllStoredByParent(EntityUid entity, string parentproto)
    {
        return FindStoredByParent(entity, parentproto, firstMatchReturn: false);
    }

    private EntityUid[] FindStoredByParent(EntityUid entity, string parentproto, bool firstMatchReturn)
    {
        return FindStoredInternal(entity, Inherits, firstMatchReturn);

        bool Inherits(EntityUid ent)
        {
            return Prototype(ent)?.Parents?.Contains(parentproto) is true;
        }
    }

    #endregion

    #region By entity tag

    /// <summary>
    /// Get the first entity matching the tag ID
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="tag">tag assigned to entity</param>
    /// <returns>First entity matching prototype id</returns>
    public EntityUid[] FindFirstStoredByTag(EntityUid entity, string tag)
    {
        return FindStoredByTag(entity, tag, true);
    }

    /// <summary>
    /// Get all entities matching the tag ID
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="tag">tag assigned to entity</param>
    /// <returns>HashSet of entities matching prototype id</returns>
    public EntityUid[] FindAllStoredByTag(EntityUid entity, string tag)
    {
        return FindStoredByTag(entity, tag, false);
    }

    private EntityUid[] FindStoredByTag(EntityUid entity, string tag, bool firstMatchReturn)
    {
        return FindStoredInternal(entity, HasTag, firstMatchReturn);

        bool HasTag(EntityUid ent)
        {
            return _tag.HasTag(ent, tag);
        }
    }

    #endregion

    #region By prototype ID

    /// <summary>
    /// Get the first entity matching the prototype ID
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="prototype">prototype ID</param>
    /// <returns>First entity matching prototype id</returns>
    public EntityUid[] FindFirstStored(EntityUid entity, string prototype)
    {
        return FindStoredByPrototype(entity, prototype, firstMatchReturn: true);
    }

    /// <summary>
    /// Get all entities matching the prototype ID
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="prototype">prototype ID</param>
    /// <returns>HashSet of entities matching prototype id</returns>
    public EntityUid[] FindAllStored(EntityUid entity, string prototype)
    {
        return FindStoredByPrototype(entity, prototype, firstMatchReturn: false);
    }

    private EntityUid[] FindStoredByPrototype(EntityUid entity, string prototype, bool firstMatchReturn)
    {
        return FindStoredInternal(entity, Match, firstMatchReturn);

        bool Match(EntityUid ent)
        {
            return Prototype(ent)?.ID == prototype;
        }
    }

    #endregion

    #region Component

    /// <summary>
    /// Get the first entity that contains the given component
    /// </summary>
    /// <returns>First entity with the requested component</returns>
    public EntityUid[] FindFirstStored<T>(EntityUid entity) where T : IComponent
    {
        return FindStoredByComponent<T>(entity, firstMatchReturn: true);
    }

    /// <summary>
    /// Get all entities that contain the given component
    /// </summary>
    /// <returns>Array of entities with the requested component</returns>
    public EntityUid[] FindAllStored<T>(EntityUid entity) where T : IComponent
    {
        return FindStoredByComponent<T>(entity, firstMatchReturn: false);
    }

    private EntityUid[] FindStoredByComponent<T>(EntityUid entity, bool firstMatchReturn) where T : IComponent
    {
        return FindStoredInternal(entity, HasComp<T>, firstMatchReturn);
    }

    /// <inheritdoc cref="FindFirstStored{T}" />
    public EntityUid[] FindFirstStored(EntityUid entity, IComponent component)
    {
        return FindStoredByComponent(entity, component, firstMatchReturn: true);
    }

    /// <inheritdoc cref="FindAllStored{T}" />
    public EntityUid[] FindAllStored(EntityUid entity, IComponent component)
    {
        return FindStoredByComponent(entity, component, firstMatchReturn: false);
    }

    private EntityUid[] FindStoredByComponent(EntityUid entity, IComponent component, bool firstMatchReturn)
    {
        return FindStoredInternal(entity, uid => HasComp(uid, component.GetType()), firstMatchReturn);
    }

    #endregion
}
