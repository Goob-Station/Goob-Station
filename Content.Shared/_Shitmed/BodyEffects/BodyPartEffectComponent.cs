using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Shitmed.BodyEffects;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentPause]
public sealed partial class BodyPartEffectComponent : Component
{
    /// <summary>
    ///     The components that are active on the part and will be refreshed every 5s
    /// </summary>
    [DataField]
    public ComponentRegistry Active = new();

    /// <summary>
    ///     How long to wait between each refresh.
    ///     Effects can only last at most this long once the organ is removed.
    /// </summary>
    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(5);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextUpdate = TimeSpan.Zero;
}