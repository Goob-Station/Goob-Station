using System.Numerics;

namespace Content.Goobstation.Shared.Cinematic;

/// <summary>
/// Raised on the focus entity each frame while the local player is pulled into its cinematic,
/// and while the effects are still fading out.
/// </summary>
[ByRefEvent]
public record struct CinematicUpdatedEvent(float Strength, float Remaining)
{
    public Vector2 EyeOffset;
}
