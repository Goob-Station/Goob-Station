using Robust.Shared.GameStates;

// god the name
namespace Content.Goobstation.Shared.Stealth;
[RegisterComponent, NetworkedComponent]
public sealed partial class ForcedStealthStatusEffectComponent : Component
{
    [DataField] public float Visibility = 0f;
}
