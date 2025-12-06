using Content.Shared.Chemistry;
using Content.Shared.EntityEffects;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Goobstation.Shared.Disease.Components;

/// <summary>
/// A disease effect that executes reagent effects.
/// Severity from DiseaseEffectComponent automatically scales the effect strength.
/// </summary>
[RegisterComponent]
public sealed partial class DiseaseReagentEffectComponent : ScalingDiseaseEffect
{
    /// <summary>
    /// The reagent effects to execute when Rthis disease effect triggers
    /// </summary>
    [DataField(required: true, serverOnly: true)]
    public List<EntityEffect> Effects = [];

    /// <summary>
    /// Base quantity to pass to reagent effects (gets multiplied by Severity from DiseaseEffectComponent)
    /// </summary>
    [DataField]
    public float BaseQuantity = 1.0f;

    /// <summary>
    /// Additional multiplier on top of severity scaling
    /// Use this to tune how strongly severity affects this particular effect
    /// </summary>
    [DataField]
    public float SeverityMultiplier = 1.0f;

    /// <summary>
    /// Whether to use the effect scale or not, some reagent effects do not scale.
    /// </summary>
    [DataField]
    public bool Scale = true;
}
