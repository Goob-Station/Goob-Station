using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Inventory;

namespace Content.Goobstation.Server.Lollypop;

[RegisterComponent]
public sealed partial class LollypopComponent : Component
{
    [DataField]
    public FixedPoint2 Amount = FixedPoint2.New(0.10);

    [DataField]
    public EntityUid? HeldBy = null;

    [DataField]
    public SlotFlags CheckSlot = SlotFlags.MASK;

    [DataField]
    public bool DeleteOnEmpty = true; // for unique lollipops that don't get turned into trash when empty
}
