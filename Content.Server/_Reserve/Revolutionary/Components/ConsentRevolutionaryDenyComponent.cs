namespace Content.Server._Reserve.Revolutionary.Components;

/// <summary>
///  Doesn't allow entity to be converter to revolution
/// </summary>

[RegisterComponent]
public sealed partial class ConsentRevolutionaryDenyComponent : Component
{
    /// <summary>
    /// Text that will appear to convertor when trying to convert entity with this component
    /// </summary>
    [DataField]
    public string OnConversionAttemptText = "rev-consent-convert-failed-convert-block";
}
