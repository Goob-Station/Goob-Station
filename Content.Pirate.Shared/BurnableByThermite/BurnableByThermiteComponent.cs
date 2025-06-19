namespace Content.Pirate.Shared.BurnableByThermite;

/// <summary>
/// Component for structures, that allows thermite to burn trough them.
/// </summary>
[RegisterComponent]
public sealed partial class BurnableByThermiteComponent : Component
{
    /// <summary>
    /// Time it takes to burn through the structure since full ignition. In seconds.
    /// </summary>
    [DataField] public float BurnTime = 10f;
    /// <summary>
    /// Time it takes to fully ignite thermite. In seconds.
    /// </summary>
    [DataField] public float IgnitionTime = 1f;
}
