namespace Content.Goobstation.Server.Weapons;

[RegisterComponent]
public sealed partial class GibThisGuyComponent : Component
{
    [DataField]
    public List<string> OcNames = new();
    [DataField]
    public List<string> IcNames = new();
    [DataField]
    public bool RequireBoth = false;
}
