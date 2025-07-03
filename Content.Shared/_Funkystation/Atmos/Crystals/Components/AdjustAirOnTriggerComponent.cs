using Content.Shared.Atmos;
using Robust.Shared.GameStates;

namespace Content.Shared._Funkystation.Atmos.Components;

/// <summary>
/// When triggered, releases specified gases or adjusts temperature in a given range around the entity.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AdjustAirOnTriggerComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public Dictionary<Gas, float> GasAdjustments = new();

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float? Temperature = null;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float Range = 1.0f;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float Probability = 1.0f;
}
