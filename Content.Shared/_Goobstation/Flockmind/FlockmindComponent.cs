using Content.Shared.StatusIcon;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Content.Shared.NPC.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Prototypes;
using Content.Shared.Alert;

namespace Content.Shared.Flockmind;

[NetSerializable, Serializable]
public enum FlockmindVisualLayers : byte
{
    Digit1, Digit2, Digit3, Digit4
}
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

    public float Resource = 150;

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public ProtoId<FactionIconPrototype> StatusIcon { get; set; } = "Flock";
    [DataField] public ProtoId<AlertPrototype> ResourceAlert = "Resource";
    //[DataField] public ProtoId<AlertPrototype> ComputeAlert = "Compute";
}

