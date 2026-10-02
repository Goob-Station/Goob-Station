using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Sandevistan;

/// <summary>
/// Plays the timestop swirl effect out of this entity for everyone who can see it.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SandevistanTimestopBurstComponent : Component
{
    [DataField]
    public TimeSpan Lasts = TimeSpan.FromSeconds(0.5f);

    [DataField]
    public TimeSpan StartedAt;
}
