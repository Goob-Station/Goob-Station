using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AreaGraspEffectComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Size;

    [DataField, AutoNetworkedField]
    public TimeSpan SpawnTime = TimeSpan.Zero;

    [DataField]
    public float FadeTime = 0.5f;

    [DataField]
    public Color EffectColor = Color.FromHex("#e88cff");
}
