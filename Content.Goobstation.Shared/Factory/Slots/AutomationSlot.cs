using Content.Goobstation.Shared.Factory.Filters;
using Content.Shared.DeviceLinking;
using Robust.Shared.Prototypes;
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

    /// <summary>
    /// The input port for this slot, or null if can only be used as an output.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<SinkPortPrototype>? Input;

    /// <summary>
    /// The output port for this slot, or null if can only be used as an input.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<SourcePortPrototype>? Output;

    public void Initialize()
    {
        IoCManager.InjectDependencies(this);
    }

    /// <summary>
    /// Try to insert an item into the slot, returning true if it was removed from its previous container.
    /// </summary>
    public abstract bool Insert(EntityUid uid, EntityUid item);

    /// <summary>
    /// Get an item that can be taken from this slot, which has to match a given filter.
    /// If there are multiple items, which one returned is arbitrary and should not be relied upon.
    /// This should be "pure" and not actually modify anything.
    /// </summary>
    public abstract EntityUid? GetItem(EntityUid uid, AutomationFilter? filter);
}
