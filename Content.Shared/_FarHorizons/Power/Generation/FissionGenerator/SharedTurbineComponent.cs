using Robust.Shared.Serialization;

namespace Content.Shared._FarHorizons.Power.Generation.FissionGenerator;

[Serializable, NetSerializable]
public enum TurbineUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class TurbineBuiState : BoundUserInterfaceState
{
    //Indicator Lights
    public bool Overspeed;
    public bool Stalling;
    public bool Overtemp;
    public bool Undertemp;

    // Speed
    public float RPM;
    public float BestRPM;

    // Flow rate
    public float FlowRateMin;
    public float FlowRateMax;
    public float FlowRate;

    //Stator load
    public float StatorLoadMin;
    public float StatorLoadMax;
    public float StatorLoad;
}

[Serializable, NetSerializable]
public sealed class TurbineChangeFlowRateMessage(float flowRate) : BoundUserInterfaceMessage
{
    public float FlowRate { get; } = flowRate;
}

[Serializable, NetSerializable]
public sealed class TurbineChangeStatorLoadMessage(float statorLoad) : BoundUserInterfaceMessage
{
    public float StatorLoad { get; } = statorLoad;
}