namespace Content.Server._Goobstation.Weapons.BatterySlotRequiresItemToggle;

[RegisterComponent]
public sealed partial class BatterySlotRequiresToggleComponent : Component
{
    [DataField(required: true)]
    public string ItemSlot = string.Empty;

    [DataField]
    public bool Inverted;
}
