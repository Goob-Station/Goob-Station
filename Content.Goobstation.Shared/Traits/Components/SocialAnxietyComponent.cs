using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Traits.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class SocialAnxietyComponent : Component
{
    [DataField] public float DownedTime = 3;
}
