using Content.Shared.StatusIcon;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Content.Shared.NPC.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared.Flockmind;

[RegisterComponent, NetworkedComponent]
//[AutoGenerateComponentState]
public sealed partial class FlockmindComponent : Component
{
public readonly List<ProtoId<EntityPrototype>> BaseFlockmindActions = new()
    {
        "ControlDrone",
        "DesignateTarget",
        "PartitionMind",
        "RadioStun",
        "RepairBurst",
        "GateCrash",
        "DiffractDrone"
    };

    public enum ResourceVisualLayers : byte
    {
        Digit1, Digit2, Digit3, Digit4
    }

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public ProtoId<FactionIconPrototype> StatusIcon { get; set; } = "Flock";

}

