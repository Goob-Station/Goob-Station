namespace Content.Goobstation.Server.DelayedTeleport;

[RegisterComponent]
public sealed partial class DelayedTeleportComponent : Component
{
    public EntityUid MapUid;
    public EntityUid? GridUid;
    public float Delay;
    public float Elapsed;
}
