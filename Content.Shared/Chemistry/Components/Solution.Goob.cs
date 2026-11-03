using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;

namespace Content.Shared.Chemistry.Components;

public sealed partial class Solution
{
    public Color GetPuddleColorWithout(IPrototypeManager? protoMan, params ProtoId<ReagentPrototype>[] without)
    {
        if (Volume == FixedPoint2.Zero)
            return Color.Transparent;

        IoCManager.Resolve(ref protoMan);

        Color mixColor = default;
        var runningTotalQuantity = FixedPoint2.New(0);
        var first = true;

        foreach (var (reagent, quantity) in Contents)
        {
            if (Array.IndexOf(without, reagent.Prototype) >= 0)
                continue;

            runningTotalQuantity += quantity;

            if (!protoMan.TryIndex(reagent.Prototype, out ReagentPrototype? proto))
                continue;

            var color = proto.PuddleColor ?? proto.SubstanceColor;

            if (first)
            {
                first = false;
                mixColor = color;
                continue;
            }

            var interpolateValue = quantity.Float() / runningTotalQuantity.Float();
            mixColor = Color.InterpolateBetween(mixColor, color, interpolateValue);
        }

        return mixColor;
    }
}
