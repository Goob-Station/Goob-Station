namespace Content.Server.Goobstation.WeaponRandomExplode;

[RegisterComponent]
public sealed partial class WeaponRandomExplodeComponent : Component
{
    [DataField, AutoNetworkedField]
    public float explosionChance;

    [DataField, AutoNetworkedField]
    public float multiplyByCharge;
}