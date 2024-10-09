namespace Content.Server._Goobstation.ChronoLegionnaire.Components;

/// <summary>
/// Marks gun entity that will return in owner hand or belt when thrown
/// </summary>
[RegisterComponent]
public sealed partial class StasisGunComponent : Component
{
    /// <summary>
    /// Slot which weapon will attempt to return
    /// </summary>
    [DataField]
    public string ReturningSlot = "belt";
}
