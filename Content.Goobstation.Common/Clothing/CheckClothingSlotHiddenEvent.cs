namespace Content.Goobstation.Common.Clothing;

[ByRefEvent]
public record struct CheckClothingSlotHiddenEvent(string Slot, bool Visible = true);
