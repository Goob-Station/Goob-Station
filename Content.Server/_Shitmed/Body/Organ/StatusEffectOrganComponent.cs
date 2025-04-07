using Content.Shared.StatusEffect;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._Shitmed.Body.Organ;

[RegisterComponent, Access(typeof(StatusEffectOrganSystem))]
[AutoGenerateComponentPause]
public sealed partial class StatusEffectOrganComponent : Component
{
    /// <summary>
    /// List of status effects and components to refresh while the organ is installed.
    /// </summary>
    [DataField(required: true)]
    public Dictionary<ProtoId<StatusEffectPrototype>, string> Refresh = new();

    /// <summary>
    /// How long to wait between each refresh.
    /// Effects can only last at most this long once the organ is removed.
    /// </summary>
    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(5);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextUpdate = TimeSpan.Zero;
}
