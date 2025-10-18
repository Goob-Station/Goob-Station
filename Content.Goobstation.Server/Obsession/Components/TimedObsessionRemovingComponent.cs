namespace Content.Goobstation.Server.Obsession;

[RegisterComponent]
public sealed partial class TimedObsessionRemovingComponent : Component
{
    public TimeSpan RemoveTime = TimeSpan.Zero;
}
