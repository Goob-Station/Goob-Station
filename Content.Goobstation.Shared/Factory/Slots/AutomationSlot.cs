using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Goobstation.Shared.Factory.Slots;

/// <summary>
/// An abstraction over some way to insert/take an item from a machine.
/// For these methods user is the machine that is doing the automation not a player.
/// </summary>
[ImplicitDataDefinitionForInheritors]
public abstract partial class AutomationSlot
{
    [Dependency] public readonly IEntityManager EntMan = default!;

    public void Initialize()
    {
        IoCManager.InjectDependencies(this);
    }

    /// <summary>
    /// Try to insert an item into the slot, returning true if it was removed from its previous container.
    /// </summary>
    public abstract bool Insert(EntityUid uid, EntityUid item, EntityUid user);

    /// <summary>
    /// Get an item that can be taken from this slot.
    /// If there are multiple items, which one returned is arbitrary and should not be relied upon.
    /// This should be "pure" and not actually modify anything.
    /// </summary>
    public abstract EntityUid? GetItem(EntityUid uid, EntityUid user);
}
