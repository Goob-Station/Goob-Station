namespace Content.Shared._White.Xenomorphs.Larva;

[RegisterComponent]
public sealed partial class XenomorphLarvaComponent : Component
{
    [DataField]
    public TimeSpan BurstDelay = TimeSpan.FromSeconds(5);

    [ViewVariables]
    public EntityUid? Victim;
}
