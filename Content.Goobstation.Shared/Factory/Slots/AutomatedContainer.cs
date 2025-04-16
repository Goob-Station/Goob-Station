using Content.Goobstation.Shared.Factory.Filters;
using Robust.Shared.Containers;

namespace Content.Goobstation.Shared.Factory.Slots;

/// <summary>
/// Abstraction over a <see cref="BaseContainer"/> on the machine.
/// </summary>
public sealed partial class AutomatedContainer : AutomationSlot
{
    /// <summary>
    /// The ID of the container to use.
    /// </summary>
    [DataField(required: true)]
    public string ContainerId = string.Empty;

    private SharedContainerSystem? _containers;

    // Dependency doesnt work for whatever reason
    public SharedContainerSystem Containers
    {
        get
        {
            _containers ??= EntMan.System<SharedContainerSystem>();
            return _containers;
        }
    }

    private BaseContainer? _container;
    public BaseContainer GetContainer(EntityUid uid)
    {
        _container ??= Containers.GetContainer(uid, ContainerId);
        return _container;
    }

    public override bool Insert(EntityUid uid, EntityUid item)
    {
        return base.Insert(uid, item) && Containers.Insert(item, GetContainer(uid));
    }

    public override bool CanInsert(EntityUid uid, EntityUid item)
    {
        return base.CanInsert(uid, item) && Containers.CanInsert(item, GetContainer(uid));
    }

    public override EntityUid? GetItem(EntityUid uid, AutomationFilter? filter)
    {
        var container = GetContainer(uid);
        var count = container.Count;

        foreach (var item in container.ContainedEntities)
        {
            if (filter?.IsAllowed(item) ?? true)
                return item;
        }

        return null;
    }
}
