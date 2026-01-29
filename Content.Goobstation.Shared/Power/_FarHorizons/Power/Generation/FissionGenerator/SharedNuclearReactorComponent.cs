using Content.Shared._FarHorizons.Power.Generation.FissionGenerator;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Power._FarHorizons.Power.Generation.FissionGenerator;

[Serializable, NetSerializable]
public enum NuclearReactorUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class NuclearReactorBuiState : BoundUserInterfaceState
{
    public Dictionary<Vector2i, ReactorSlotBUIData> SlotData = [];

    public int GridWidth = 0;
    public int GridHeight = 0;

    public string? ItemName;

    public float ReactorTemp = 0;
    public float ReactorRads = 0;
    public float ReactorTherm = 0;
    public float ControlRodActual = 0;
    public float ControlRodSet = 0;
}

[Serializable, NetSerializable, DataDefinition]
public sealed partial class ReactorSlotBUIData
{
    public double Temperature = 0f;
    public int NeutronCount = 0;
    public string IconName = "base";
    public string PartName = "empty";

    public float NeutronRadioactivity = 0f;
    public float Radioactivity = 0f;
    public float SpentFuel = 0f;
}

[Serializable, NetSerializable]
public sealed class ReactorItemActionMessage(Vector2d position) : BoundUserInterfaceMessage
{
    public Vector2d Position { get; } = position;
}

[Serializable, NetSerializable]
public sealed class ReactorEjectItemMessage() : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class ReactorControlRodModifyMessage(float change) : BoundUserInterfaceMessage
{
    public float Change { get; } = change;
}