using Robust.Shared.GameObjects;

namespace Content.Goobstation.Shared.Doodons;

/// <summary>
/// Generic Doodon production machine.
/// </summary>
[RegisterComponent]
public sealed partial class DoodonMachineComponent : Component
{
    [DataField(required: true)]
    public string OutputPrototype = default!;

    [DataField(required: true)]
    public string ResinStack = default!;

    [DataField]
    public int ResinCost = 1;

    [DataField]
    public float ProcessTime = 5f;

    [DataField]
    public bool RespectPopulationCap = true;

    [DataField]
    public bool SpawnOnMapInit = false;

    /// <summary>
    /// How much resin is currently stored.
    /// </summary>
    public int StoredResin;

    /// <summary>
    /// Is the machine actively processing?
    /// </summary>
    public bool Processing;

    /// <summary>
    /// Accumulated processing time.
    /// </summary>
    public float Accumulator;
}
