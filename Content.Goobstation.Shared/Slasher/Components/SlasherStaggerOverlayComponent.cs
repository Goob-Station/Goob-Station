using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Slasher.Components;

/// <summary>
/// Draws an expanding shockwave ring around this entity.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SlasherStaggerOverlayComponent : Component
{
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(1.1);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField]
    public TimeSpan EndTime;

    [ViewVariables]
    public TimeSpan? LocalStartTime;

    [DataField]
    public float Range = 3.5f;

    [DataField]
    public string ShockwaveShader = "SlasherStagger";

    [DataField]
    public Color RingColor = new(0.22f, 0.02f, 0.34f);
}
