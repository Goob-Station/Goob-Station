using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._ES.Fluids;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;

namespace Content.Shared.Fluids;

public abstract partial class SharedPuddleSystem
{
    protected static readonly ProtoId<ESPuddleSpriteSetPrototype> DefaultPuddleSpriteSet = "Default";

    private ProtoId<ESPuddleSpriteSetPrototype> GetPuddleSpriteSet(Solution solution)
    {
        var spriteSets = new Dictionary<ProtoId<ESPuddleSpriteSetPrototype>, FixedPoint2>();
        foreach (var (reagent, quantity) in solution.Contents)
        {
            if (!_prototypeManager.TryIndex<ReagentPrototype>(reagent.Prototype, out var proto))
                continue;

            spriteSets[proto.PuddleSpriteSet] = spriteSets.GetValueOrDefault(proto.PuddleSpriteSet) + quantity;
        }

        var best = DefaultPuddleSpriteSet;
        var bestQuantity = FixedPoint2.Zero;
        foreach (var (set, quantity) in spriteSets)
        {
            if (quantity <= bestQuantity)
                continue;

            best = set;
            bestQuantity = quantity;
        }

        return best;
    }
}
