namespace Content.Goobstation.Shared.Traits.Components;

[RegisterComponent]
public sealed partial class MovementImpairedCorrectionComponent : Component
{
    /// <summary>
    /// How much should the impaired speed be fixed by this component?
    /// </summary>
    /// <remarks>
    /// If set to zero, remove the impaired speed entirely.
    /// </remarks>
    [DataField]
    public float SpeedCorrection = 0f;
}
