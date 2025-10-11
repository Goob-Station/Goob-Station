using Content.Goobstation.Shared.Factory.Slots;
using Content.Shared.DeviceLinking;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Factory;

public partial interface IExclusiveSlotComponent : IComponent
{
    /// <summary>
    /// Port on this machine that other machines link to.
    /// </summary>
    string PortId { get; }

    /// <summary>
    /// Machine linked to <see cref="Port"/>.
    /// </summary>
    EntityUid? LinkedMachine { get; set; }

    /// <summary>
    /// The source or sink port of the linked machine.
    /// </summary>
    /// <remarks>
    /// Not using protoid as it can be either a sink or source, and prototypes don't set it anyway.
    /// </remarks>
    string? LinkedPort { get; set; }

    /// <summary>
    /// The resolved automation slot of the linked machine.
    /// Updated by <c>UpdateSlot</c> as this is not directly networked.
    /// </summary>
    AutomationSlot? LinkedSlot { get; set; }

    bool IsInput { get; }
}

/// <summary>
/// Maintains exclusive linkage to an input port.
/// Only 1 machine can be linked to it, and non-automation slots are forbidden from being linked.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(ExclusiveSlotsSystem))]
[AutoGenerateComponentState(true)]
public sealed partial class ExclusiveInputSlotComponent : Component, IExclusiveSlotComponent
{
    [DataField(required: true)]
    public ProtoId<SinkPortPrototype> Port;
    public string PortId => Port;

    [DataField, AutoNetworkedField]
    public EntityUid? LinkedMachine { get; set; }

    [DataField, AutoNetworkedField]
    public string? LinkedPort { get; set; }

    [ViewVariables]
    public AutomationSlot? LinkedSlot { get; set; }

    public bool IsInput => true;
}

/// <summary>
/// Maintains exclusive linkage to an output port.
/// Only 1 machine can be linked to it, and non-automation slots are forbidden from being linked.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(ExclusiveSlotsSystem))]
[AutoGenerateComponentState(true)]
public sealed partial class ExclusiveOutputSlotComponent : Component, IExclusiveSlotComponent
{
    [DataField(required: true)]
    public ProtoId<SourcePortPrototype> Port;
    public string PortId => Port;

    [DataField, AutoNetworkedField]
    public EntityUid? LinkedMachine { get; set; }

    [DataField, AutoNetworkedField]
    public string? LinkedPort { get; set; }

    [ViewVariables]
    public AutomationSlot? LinkedSlot { get; set; }

    public bool IsInput => false;
}
