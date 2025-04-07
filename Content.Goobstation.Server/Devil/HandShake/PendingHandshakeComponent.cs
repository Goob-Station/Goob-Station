namespace Content.Goobstation.Server.Contract;

[RegisterComponent]
public sealed partial class PendingHandshakeComponent : Component
{
    [DataField]
    public EntityUid? Offerer;

    [DataField]
    public TimeSpan ExpiryTime;
}
