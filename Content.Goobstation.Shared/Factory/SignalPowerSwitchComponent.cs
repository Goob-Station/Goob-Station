using Content.Shared.DeviceLinking;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Factory;

/// <summary>
/// Adds toggle/on/off sinks and powered source ports.
/// Allows for signal control similar to manual <c>PowerSwitch</c>.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(SignalPowerSwitchSystem))]
public sealed partial class SignalPowerSwitchComponent : Component
{
    [DataField]
    public ProtoId<SinkPortPrototype> TogglePort = "Toggle";

    [DataField]
    public ProtoId<SinkPortPrototype> OnPort = "On";

    [DataField]
    public ProtoId<SinkPortPrototype> OffPort = "Off";

    [DataField]
    public ProtoId<SourcePortPrototype> PoweredPort = "Powered";
}
