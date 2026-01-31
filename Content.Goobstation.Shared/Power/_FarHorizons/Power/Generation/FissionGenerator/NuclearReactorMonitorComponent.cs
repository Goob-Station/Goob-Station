using Content.Shared.DeviceLinking;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._FarHorizons.Power.Generation.FissionGenerator;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NuclearReactorMonitorComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public NetEntity? reactor;

    [DataField]
    public ProtoId<SinkPortPrototype> LinkingPort = "NuclearReactorDataReceiver";
}