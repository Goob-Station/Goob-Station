using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wizard.Spellblade;

[RegisterComponent]
public sealed partial class LightningSpellbladeEnchantmentComponent : Component
{
    [DataField]
    public float ShockDamage = 30f;

    [DataField]
    public float ShockTime = 1f;

    [DataField]
    public float Range = 4f;

    [DataField]
    public int BoltCount = 3;

    [DataField]
    public int ArcDepth = 1;

    [DataField]
    public float Siemens = 1f;

    [DataField]
    public EntProtoId LightningPrototype = "HyperchargedLightning";
}
