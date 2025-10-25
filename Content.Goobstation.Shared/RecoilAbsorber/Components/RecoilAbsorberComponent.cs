using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.RecoilAbsorber;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RecoilAbsorberComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Modifier = 0.3f;
}
