namespace Content.Server.DeltaV.AACTablet;

[RegisterComponent]
public sealed partial class AACTabletComponent : Component
{
    // Minimum time between each phrase, to prevent spam
    [DataField]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(1);

    // Time that the next phrase can be sent.
    [DataField]
    public TimeSpan NextPhrase;
}
