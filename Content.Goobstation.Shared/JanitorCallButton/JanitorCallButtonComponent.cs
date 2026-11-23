namespace Content.Goobstation.Shared.JanitorCallButton;

[RegisterComponent]
public sealed partial class JanitorCallButtonComponent : Component
{
    [DataField]
    public bool Enabled;
}

public readonly record struct JanitorCallButtonPressedEvent;
