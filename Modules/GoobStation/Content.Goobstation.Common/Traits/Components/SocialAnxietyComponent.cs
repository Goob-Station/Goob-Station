using Robust.Shared.GameStates;

namespace Content.Goobstation.Common.Traits.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class SocialAnxietyComponent : Component
{
    [DataField] public float DownedTime = 3;
}
