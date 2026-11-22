using Content.Shared.DeviceLinking;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitmed.Analyzer;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class BodyScannerComponent : Component
{
    [DataField]
    public ProtoId<SinkPortPrototype> OperatingTablePort = "OperatingTableReceiver";

    [DataField, AutoNetworkedField]
    public EntityUid? OperatingTable;
}