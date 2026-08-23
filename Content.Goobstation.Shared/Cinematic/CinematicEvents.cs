using System.Numerics;

namespace Content.Goobstation.Shared.Cinematic;

/// <summary>
/// Raised on the focus entity each frame while the local player is pulled into its cinematic.
/// </summary>
[ByRefEvent]
public record struct CinematicUpdatedEvent(float Strength)
{
    public Vector2 EyeOffset;
}
