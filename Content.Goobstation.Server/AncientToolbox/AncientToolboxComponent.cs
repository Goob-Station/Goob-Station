namespace Content.Goobstation.Server.AncientToolbox;

[RegisterComponent]
public sealed partial class AncientToolboxComponent : Component
{
    [DataField] public int CrystalsPerDamageBonus = 5;
    [DataField] public int BonusDamage = 0;
}
