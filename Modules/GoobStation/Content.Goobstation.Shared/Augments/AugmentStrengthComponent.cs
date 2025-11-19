using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Augments;

/// <summary>
/// Multiplies melee damage, armed and unarmed, when activated.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(AugmentStrengthSystem))]
public sealed partial class AugmentStrengthComponent : Component
{
    /// <summary>
    /// What to multiply damage by when activated.
    /// </summary>
    [DataField]
    public float Modifier = 1.25f;
}
