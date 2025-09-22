using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Inventory;

namespace Content.Goobstation.Server.Lollypop;

[RegisterComponent]
public sealed partial class LollypopComponent : Component
{
    [DataField]
    public FixedPoint2 Ammount = FixedPoint2.New(0.1);

    [DataField]
    public EntityUid? HeldBy = null;

    [DataField]
    public SlotFlags CheckSlot = SlotFlags.MASK;

    [DataField]
    public TimeSpan NextBite = TimeSpan.Zero;

    [DataField]
    public TimeSpan BiteInterval = TimeSpan.FromSeconds(3);

    [DataField]
    public bool DeleteOnEmpty = true; // for unique lollipops that don't get turned into trash when empty
}
