using Content.Shared.DeviceLinking;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Teleportation.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class TelesciComputerComponent : Component
{
    [DataField, AutoNetworkedField]
    public float X = 1; //TODO make into vector

    [DataField, AutoNetworkedField]
    public float Y = 1;

    /// <summary>
    /// The machine linking port for the teleporter
    /// </summary>
    [DataField]
    public ProtoId<SourcePortPrototype> LinkingPort = "Output";

    /// <summary>
    /// The teleporter entity the console is linked.
    /// Can be null if not linked.
    /// </summary>
    [DataField, AutoNetworkedField]
    public NetEntity? TeleporterEntity;

    [DataField, AutoNetworkedField]
    public TimeSpan CooldownTime = TimeSpan.Zero;
}
