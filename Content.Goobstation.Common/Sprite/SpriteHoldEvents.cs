namespace Content.Goobstation.Common.Sprite;

/// <summary>
/// Raised on an entity before something sets its sprite for its own state.
/// This will be used later on for cinematics.
/// </summary>
[ByRefEvent]
public record struct SpriteHeldEvent
{
    public bool Held;
}

[ByRefEvent]
public readonly record struct SpriteReleasedEvent;
