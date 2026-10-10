using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Mech.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class MechStompOverlayComponent : Component
{
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(1.8);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan EndTime;

    [ViewVariables]
    public TimeSpan? LocalStartTime;

    [DataField]
    public TimeSpan SpriteLiftTiming = TimeSpan.FromSeconds(0.22);

    [DataField]
    public float SpriteLiftHeight = 0.14f;

    [DataField]
    public SoundSpecifier SlamSound = new SoundCollectionSpecifier("MetalSlam", AudioParams.Default.WithVariation(0.08f));


    [ViewVariables]
    public bool Landed;

    [DataField, AutoNetworkedField]
    public EntityCoordinates Origin;


    [DataField, AutoNetworkedField]
    public float DustRange = 1.6f;

    [DataField]
    public string ShockwaveShader = "MechStompShockwave";

    [DataField]
    public Color DustColor = Color.FromHex("#a89e8f");
}
