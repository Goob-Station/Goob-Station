namespace Content.Goobstation.Common.Clothing;
/// <summary>
///     Represents the results of a lookup in terms of "equippability" for an entity considering their species, body part availability, and clothing presence.
/// </summary>
public enum EquipAbility : int
{
    CannotEquip = 0,
    SlotOccupiedOrEmpty = 1,
    MissingPart = 2,
    MissingSlot = 3,
    CanEquip = 4,
}

