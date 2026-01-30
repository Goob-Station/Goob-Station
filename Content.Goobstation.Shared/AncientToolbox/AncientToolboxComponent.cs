namespace Content.Goobstation.Shared.AncientToolbox;

[RegisterComponent]
public sealed partial class AncientToolboxComponent : Component
{
    [DataField("crystalsPerDamageBonus")]
    public float CrystalsPerDamageBonus = 5f;
    [ViewVariables(VVAccess.ReadOnly)]
    public int BonusDamage = 0;
}
