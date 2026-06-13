using Content.Shared.Mobs;

namespace Content.Server._Obelisk.Species.Components;

/// <summary>
/// Passively adds or takes away heat from the entity.
/// </summary>
[RegisterComponent]
public sealed partial class PassiveHeatGenerationComponent : Component
{
    /// <summary>
    /// Amount of heat being generated or taken away in watts.
    /// </summary>
    [DataField]
    public float Watts;

    /// <summary>
    /// Each mob state can have its own custom modifier. This will be applied to <see cref="Joules"/>
    /// Set to 0 for the modifier to not apply at all.
    /// </summary>
    [DataField]
    public Dictionary<MobState, float>? MobStateModifier;

    /// <summary>
    /// Minimum temperature for the effect to apply.
    /// </summary>
    [DataField]
    public float? MinimumTemperature;

    /// <summary>
    /// Maximum temperature for the effect to apply
    /// </summary>
    [DataField]
    public float? MaximumTemperature;

    /// <summary>
    /// If the heat will ignore heat resistance or if it will be taken into account.
    /// </summary>
    [DataField]
    public bool IgnoreHeatResistance;
}
