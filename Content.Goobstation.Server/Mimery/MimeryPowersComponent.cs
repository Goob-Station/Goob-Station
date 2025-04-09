namespace Content.Goobstation.Server.Mimery;

[RegisterComponent]
public sealed partial class MimeryPowersComponent : Component
{
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? MimeryWallPower;
}
