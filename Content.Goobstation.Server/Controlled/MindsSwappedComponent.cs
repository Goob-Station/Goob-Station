namespace Content.Goobstation.Server.Controlled;

[RegisterComponent]
public sealed partial class MindSwappedComponent : Component
{
    [ViewVariables]
    public EntityUid? OriginalBody;

    [ViewVariables]
    public TimeSpan? RevertTime;
}
