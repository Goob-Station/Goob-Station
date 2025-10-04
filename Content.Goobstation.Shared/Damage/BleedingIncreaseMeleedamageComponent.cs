namespace Content.Goobstation.Shared.Damage;

/// <summary>
/// This is used for increasing melee damage depending on bleed amount
/// </summary>
[RegisterComponent]
public sealed partial class BleedingIncreaseMeleeDamageComponent : Component
{
    [DataField]
    public float Modifier = 10f; // 10x is hige, didnt manage to get the multipler above 1 in testing

}
