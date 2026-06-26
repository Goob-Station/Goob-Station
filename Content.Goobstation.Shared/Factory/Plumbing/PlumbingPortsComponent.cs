using Content.Goobstation.Shared.Factory.Slots;
using Content.Shared.DeviceLinking;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Factory.Plumbing;

/// <summary>
/// Stores plumbing links for a liquid processor.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(PlumbingLinkSystem), typeof(PlumbingPumpSystem))]
[AutoGenerateComponentState(true)]
public sealed partial class PlumbingPortsComponent : Component
{
    [DataField(required: true)]
    public ProtoId<SinkPortPrototype> Input;

    public string InputId => Input;

    [DataField(required: true)]
    public ProtoId<SourcePortPrototype> Output;

    public string OutputId => Output;

    [DataField, AutoNetworkedField]
    public EntityUid? LinkedInputMachine;

    [DataField, AutoNetworkedField]
    public string? LinkedInputPort;

    [ViewVariables]
    public AutomationSlot? LinkedInputSlot;

    [DataField, AutoNetworkedField]
    public EntityUid? LinkedOutputMachine;

    [DataField, AutoNetworkedField]
    public string? LinkedOutputPort;

    [ViewVariables]
    public AutomationSlot? LinkedOutputSlot;
}
