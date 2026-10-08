using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Sandevistan;

/// <summary>
/// Plays the timestop swirl effect out of this entity for everyone who can see it.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SandevistanTimestopBurstComponent : Component
{
    [DataField]
    public TimeSpan Lasts = TimeSpan.FromSeconds(0.5f);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan StartedAt;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan ReversedAt;
}
