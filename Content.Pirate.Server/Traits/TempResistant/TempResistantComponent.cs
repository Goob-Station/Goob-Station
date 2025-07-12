namespace Content.Pirate.Server.Traits.HeatResistant;

/// <summary>
/// Component added to entities with TraitHeatResistant and TraitColdResistant(and their oposites) traits./>
/// </summary>
[RegisterComponent]
public sealed partial class TempResistantComponent : Component
{
    /// <summary>
    ///     The multiplyer applied to <seealso cref="TemperatureComponent.HeatDamageThreshold"/>. The higher the value, the HIGHER the resistane.
    /// </summary>
    [DataField] public float HeatModifier = 1f;
    /// <summary>
    ///     The multiplyer applied to <seealso cref="TemperatureComponent.ColdDamageThreshold"/>. The higher the value, the LOWER the resistane.
    /// </summary>
    [DataField] public float ColdModifier = 1f;
}
