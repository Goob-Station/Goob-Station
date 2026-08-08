using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Slasher.Components;

/// <summary>
/// Draws an expanding shockwave ring around this entity.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SlasherStaggerOverlayComponent : Component
{
    /// <summary>
    /// How long the ring takes to expand before the component removes itself.
    /// </summary>
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(1.1);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField]
    public TimeSpan EndTime;

    /// <summary>
    /// Radius the shockwave ring expands to.
    /// </summary>
    [DataField]
    public float Range = 3.5f;

    /// <summary>
    /// Shader the shockwave ring draws with.
    /// </summary>
    [DataField]
    public string ShockwaveShader = "SlasherStagger";

    [DataField]
    public Color RingColor = new(0.22f, 0.02f, 0.34f);
}
