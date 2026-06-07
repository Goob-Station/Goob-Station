using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Projectiles;

[RegisterComponent, NetworkedComponent]
public sealed partial class DodgeEffectComponent : Component
{
    [DataField]
    public float Duration = 0.35f;
}
