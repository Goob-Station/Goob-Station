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
}
