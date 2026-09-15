using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Server.Voidwalker.Kidnapping;

/// <summary>
/// do not apply this manually
/// </summary>
[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class VoidwalkerKidnappedComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan ExitVoidTime = TimeSpan.Zero;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid OriginalMap;
}
