namespace Content.Shared.Shadowkin;

[RegisterComponent]
public sealed partial class EtherealStunItemComponent : Component
{
    [DataField]
    public float Radius = 10;

    [DataField]
    public bool DeleteOnUse = true;
}