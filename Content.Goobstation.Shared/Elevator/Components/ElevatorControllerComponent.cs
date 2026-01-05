using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using System.Numerics;
using Robust.Shared.Serialization;

[RegisterComponent]
public sealed partial class ElevatorControllerComponent : Component
{
    [DataField(required: true)]
    public List<ElevatorFloorDefinition> Floors = new();

    [DataField]
    public int CurrentFloor = 0;
}

[DataDefinition]
public sealed partial class ElevatorFloorDefinition
{
    [DataField(required: true)]
    public int Id;

    [DataField(required: true)]
    public string Name = string.Empty;

    [DataField(required: true)]
    public ResPath MapPath;

    [DataField]
    public ElevatorTarget Target = new();
}

[DataDefinition]
public sealed partial class ElevatorTarget
{
    [DataField]
    public EntityUid? AnchorEntity;

    [DataField]
    public Vector2? Coordinates;

    [DataField]
    public EntProtoId? Prototype;
}


