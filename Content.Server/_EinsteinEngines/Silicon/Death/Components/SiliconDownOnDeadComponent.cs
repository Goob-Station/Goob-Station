namespace Content.Server._EinsteinEngines.Silicon.Death;

/// <summary>
///     Marks a Silicon as becoming incapacitated when they run out of battery charge.
/// </summary>
/// <remarks>
///     Uses the Silicon System's charge states to do so, so make sure they're a battery powered Silicon.
/// </remarks>
[RegisterComponent]
public sealed partial class SiliconDownOnDeadComponent : Component
{
    /// <summary>
    ///     Is this Silicon currently dead?
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public bool Dead;
}
