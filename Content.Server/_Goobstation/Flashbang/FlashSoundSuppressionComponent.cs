namespace Content.Server._Goobstation.Flashbang;

[RegisterComponent]
public sealed partial class FlashSoundSuppressionComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float ProtectionRange = 2f;
}
