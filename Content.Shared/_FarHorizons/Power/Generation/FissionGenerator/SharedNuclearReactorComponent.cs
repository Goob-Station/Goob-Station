using Robust.Shared.Serialization;

namespace Content.Shared._FarHorizons.Power.Generation.FissionGenerator;

[Serializable, NetSerializable]
public enum NuclearReactorUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class NuclearReactorBuiState : BoundUserInterfaceState
{
    public double[] TemperatureGrid = new double[NuclearReactorComponent.ReactorGridWidth*NuclearReactorComponent.ReactorGridHeight];
    public int[] NeutronGrid = new int[NuclearReactorComponent.ReactorGridWidth*NuclearReactorComponent.ReactorGridHeight];
    public string[] IconGrid = new string[NuclearReactorComponent.ReactorGridWidth * NuclearReactorComponent.ReactorGridHeight];
    public string[] PartName = new string[NuclearReactorComponent.ReactorGridWidth * NuclearReactorComponent.ReactorGridHeight];
    public double[] PartInfo = new double[NuclearReactorComponent.ReactorGridWidth * NuclearReactorComponent.ReactorGridHeight * 3];

    public string? ItemName;

    public float ReactorTemp = 0;
    public float ReactorRads = 0;
    public float ReactorTherm = 0;
    public float ControlRodActual = 0;
    public float ControlRodSet = 0;
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