using Content.Shared.DeviceLinking;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Factory.Plumbing;

/// <summary>
/// Stores plumbing links for a liquid pump.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(PlumbingLinkSystem))]
[AutoGenerateComponentState(true)]
public sealed partial class PlumbingPortsComponent : Component
{
    [DataField(required: true)]
    public ProtoId<SinkPortPrototype> Input;

    [DataField(required: true)]
    public ProtoId<SourcePortPrototype> Output;

    [DataField, AutoNetworkedField]
    public EntityUid? LinkedInputMachine;

    [DataField, AutoNetworkedField]
    public string? LinkedInputPort;

    [DataField, AutoNetworkedField]
    public EntityUid? LinkedOutputMachine;

    [DataField, AutoNetworkedField]
    public string? LinkedOutputPort;
}
