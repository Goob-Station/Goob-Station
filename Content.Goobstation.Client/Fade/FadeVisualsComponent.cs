using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Client.Fade;

/// <summary>
/// Used for fade animations, when you need to smoothly change alpha on some entity.
/// Component is removed after animation is done.
/// </summary>
[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class FadeVisualsComponent : Component
{
    /// <summary>
    /// In how much time alpha will be changed.
    /// </summary>
    [DataField("delay", required: true)]
    public TimeSpan AlphaChangeTime;

    /// <summary>
    /// Time when alpha started fading.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan AlphaChangeStart;
}
