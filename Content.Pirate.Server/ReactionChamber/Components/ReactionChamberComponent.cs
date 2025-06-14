using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Pirate.Server.ReactionChamber.Components;

[RegisterComponent]
public sealed partial class ReactionChamberComponent : Component
{
    [DataField] public bool Active = false;
    [DataField] public float Temp = 293.15f;
    /// <summary>
    /// Heat capacity used for calculations.
    /// </summary>
    [DataField] public float HeatCapacity = 5f;
    [DataField] public float MinTemp = 73.32f;
    [DataField] public float MaxTemp = 742.15f;

    /// <summary>
    /// Actual solutions heat capacity.
    /// </summary>
    public float SolnHeatCapacity = 0f;
    public float DeltaJ = 0f;
    public bool IsAllTempRight = false;
}
