using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Serialization;

namespace Content.Shared._Pirate.Plumbing;

/// <summary>
///     UI key for the plumbing reactor interface.
/// </summary>
[Serializable, NetSerializable]
public enum PlumbingReactorUiKey : byte
{
    Key,
}

/// <summary>
///     State sent to the client to update the reactor UI.
/// </summary>
[Serializable, NetSerializable]
public sealed class PlumbingReactorBoundUserInterfaceState : BoundUserInterfaceState
{
    /// <summary>
    ///     The reagent targets the reactor is trying to accumulate.
    ///     Key is reagent prototype ID, value is target quantity.
    /// </summary>
    public Dictionary<string, FixedPoint2> ReagentTargets { get; }

    /// <summary>
    ///     Current quantities of reagents in the buffer.
    /// </summary>
    public Dictionary<string, FixedPoint2> BufferContents { get; }

    /// <summary>
    ///     Current quantities of reagents in the output.
    /// </summary>
    public Dictionary<string, FixedPoint2> OutputContents { get; }

    /// <summary>
    ///     Whether the reactor is enabled.
    /// </summary>
    public bool Enabled { get; }

    /// <summary>
    ///     Target temperature for the buffer in Kelvin.
    /// </summary>
    public float TargetTemperature { get; }

    /// <summary>
    ///     Current temperature of the buffer in Kelvin.
    /// </summary>
    public float CurrentTemperature { get; }

    public PlumbingReactorBoundUserInterfaceState(
        Dictionary<string, FixedPoint2> reagentTargets,
        Dictionary<string, FixedPoint2> bufferContents,
        Dictionary<string, FixedPoint2> outputContents,
        bool enabled,
        float targetTemperature,
        float currentTemperature)
    {
        ReagentTargets = reagentTargets;
        BufferContents = bufferContents;
        OutputContents = outputContents;
        Enabled = enabled;
        TargetTemperature = targetTemperature;
        CurrentTemperature = currentTemperature;
    }
}

/// <summary>
///     Message to toggle the reactor on/off.
/// </summary>
[Serializable, NetSerializable]
public sealed class PlumbingReactorToggleMessage : BoundUserInterfaceMessage
{
    public bool Enabled { get; }

    public PlumbingReactorToggleMessage(bool enabled)
    {
        Enabled = enabled;
    }
}

/// <summary>
///     Message to set a reagent target quantity.
/// </summary>
[Serializable, NetSerializable]
public sealed class PlumbingReactorSetTargetMessage : BoundUserInterfaceMessage
{
    public string ReagentId { get; }
    public FixedPoint2 Quantity { get; }

    public PlumbingReactorSetTargetMessage(string reagentId, FixedPoint2 quantity)
    {
        ReagentId = reagentId;
        Quantity = quantity;
    }
}

/// <summary>
///     Message to remove a reagent target.
/// </summary>
[Serializable, NetSerializable]
public sealed class PlumbingReactorRemoveTargetMessage : BoundUserInterfaceMessage
{
    public string ReagentId { get; }

    public PlumbingReactorRemoveTargetMessage(string reagentId)
    {
        ReagentId = reagentId;
    }
}

/// <summary>
///     Message to clear all reagent targets.
/// </summary>
[Serializable, NetSerializable]
public sealed class PlumbingReactorClearTargetsMessage : BoundUserInterfaceMessage;

/// <summary>
///     Message to set the target temperature.
/// </summary>
[Serializable, NetSerializable]
public sealed class PlumbingReactorSetTemperatureMessage : BoundUserInterfaceMessage
{
    public float Temperature { get; }

    public PlumbingReactorSetTemperatureMessage(float temperature)
    {
        Temperature = temperature;
    }
}
