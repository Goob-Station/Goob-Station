using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AreaMansusGraspComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan? ChannelStartTime;

    [DataField]
    public TimeSpan ChannelTime = TimeSpan.FromSeconds(2);

    [DataField]
    public float Slope = 2f;

    [DataField]
    public float MaxRange = 5f;

    [DataField]
    public float MinRange = 0f;

    [DataField]
    public Color EffectColor = Color.FromHex("#cc66e6");

    [DataField]
    public EntProtoId VisualEffect = "EldritchFlashEffect";

    [DataField]
    public SoundSpecifier ChannelSound = new SoundPathSpecifier("/Audio/Effects/tesla_consume.ogg");
}
