namespace Content.Server._Goobstation.Flashbang;

[RegisterComponent]
public sealed partial class FlashbangComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float StunTime = 2f;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float KnockdownTime = 10f;
}
