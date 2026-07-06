using Content.Goobstation.Maths.FixedPoint;
using Content.Pirate.Shared.Wetness.Components;
using Content.Pirate.Shared.Wetness.Systems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Fluids;
using Content.Shared.Hands.Components;
using Content.Shared.Inventory;
using Robust.Shared.Containers;
using Robust.Shared.Random;

namespace Content.Pirate.Server.Wetness;

/// <summary>
/// Server-authoritative drying and dripping loop. Wet clothing loses a little water on a randomized
/// cadence and occasionally drips a clean water puddle while doing so.
/// </summary>
public sealed class WetnessSystem : SharedWetnessSystem
{
    [Dependency] private readonly SharedPuddleSystem _puddle = null!;
    [Dependency] private readonly SharedContainerSystem _container = null!;
    [Dependency] private readonly IRobustRandom _random = null!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = Timing.CurTime;
        var query = EntityQueryEnumerator<WettableComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Wetness <= 0 || now < comp.NextDryTime)
                continue;

            var ent = (uid, comp);

            // Work out this tick's total loss up front — the drying step plus an optional drip — so
            // wetness is refreshed only once instead of twice.
            var dried = FixedPoint2.Min(comp.DryPerStep, comp.Wetness);
            var remaining = comp.Wetness - dried;

            var drip = FixedPoint2.Zero;
            // Each drying step has a small chance to shed a clean water drip from what's left.
            if (remaining > 0 && _random.Prob(comp.DripChance) && CanDrip(uid))
                drip = FixedPoint2.Min(comp.DripAmount, remaining);

            RemoveWetness(ent, dried + drip);

            if (drip > 0)
            {
                var water = new Solution();
                water.AddReagent("Water", drip);
                // Spilling at the item coalesces with any puddle already on its tile.
                _puddle.TrySpillAt(uid, water, out _, sound: false);
            }

            comp.NextDryTime = comp.Wetness > 0 ? now + NextDryDelay(comp) : TimeSpan.Zero;
            Dirty(uid, comp);
        }
    }

    /// <summary>
    /// Worn, held, or loose items may drip; items tucked inside storage should not puddle the floor.
    /// </summary>
    private bool CanDrip(EntityUid item)
    {
        if (!_container.TryGetContainingContainer(item, out var container))
            return true;

        return HasComp<InventoryComponent>(container.Owner) || HasComp<HandsComponent>(container.Owner);
    }
}
