using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.RecoilAbsorber;

[RegisterComponent, NetworkedComponent]
public sealed partial class RecoilAbsorberArmComponent : Component
{
    [DataField]
    public float Modifier = 0.3f;
}
