using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Voidwalker.Abilities.Kidnap.Victim;

/// <summary>
/// do not apply this manually
/// </summary>
[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class VoidwalkerKidnappedComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan ExitVoidTime;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid OriginalMap;
}
