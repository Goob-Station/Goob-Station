using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Terror.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class BonusDamageOnStatusEffectComponent : Component
{
    /// <summary>
    /// Multiply the damage by how much on stunned target.
    /// </summary>
    [DataField]
    public float DamageMultiplier;

    /// <summary>
    /// Status effects that qualify for bonus damage.
    /// </summary>
    [DataField(required: true)]
    public List<string> RequiredStatusEffects = new();
}
