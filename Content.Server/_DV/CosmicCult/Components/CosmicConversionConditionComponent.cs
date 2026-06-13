namespace Content.Server.Objectives.Components;

[RegisterComponent]
public sealed partial class CosmicConversionConditionComponent : Component
{
    /// <summary>
    ///     The amount of cultists this objective would like to be converted
    /// </summary>
    [DataField]
    public int Converted;
}
