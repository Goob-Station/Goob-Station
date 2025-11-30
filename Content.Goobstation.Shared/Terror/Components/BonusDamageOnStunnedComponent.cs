using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Terror.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class BonusDamageOnStunnedComponent : Component
{
    /// <summary>
    /// Multiply the damage by how much on stunned target.
    /// </summary>
    [DataField]
    public float DamageMultiplier = 2;
}
