namespace Content.Goobstation.Shared.Slasher.Components;

[RegisterComponent]
public sealed partial class SlasherSoulBonusAlertComponent : Component
{
    [DataField]
    public List<string> DamageDigitKeys = new() { "damage1", "damage10", "damage100" };

    [DataField]
    public List<string> ArmorDigitKeys = new() { "armor1", "armor10", "armor100" };
}
