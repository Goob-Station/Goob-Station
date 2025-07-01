namespace Content.Goobstation.Server.Explosion.Components;

/// <summary>
/// Triggers when the parent entity speaks.
/// </summary>
[RegisterComponent]
public sealed partial class TriggerOnSpeakComponent : Component
{
    /// <summary>
    ///     The range at which it listens for keywords.
    /// </summary>
    [DataField]
    public int ListenRange { get; private set; } = 4;
}

