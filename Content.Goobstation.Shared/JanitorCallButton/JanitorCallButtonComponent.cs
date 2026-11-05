namespace Content.Goobstation.Shared.JanitorCallButton;

[RegisterComponent]
public sealed partial class JanitorCallButtonComponent : Component
{
    [DataField]
    public bool enabled;
}

public readonly record struct JanitorCallButtonPressedEvent;
