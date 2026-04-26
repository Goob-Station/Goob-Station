using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Sandevistan;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class SandevistanGlitchComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan ExpiresAt;
}
