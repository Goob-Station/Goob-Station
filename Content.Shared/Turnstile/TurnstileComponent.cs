namespace Content.Shared.Turnstile;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class TurnstileComponent : Component
{
    [DataField("turnstileDirection")]
    public CardinalDirection TurnstileDirection = CardinalDirection.South;
}
