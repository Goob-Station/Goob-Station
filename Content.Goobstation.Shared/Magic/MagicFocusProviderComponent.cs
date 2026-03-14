using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Magic;

[RegisterComponent, NetworkedComponent]
public sealed partial class MagicFocusProviderComponent : Component
{
    [DataField] public int Weight = 1;
}
