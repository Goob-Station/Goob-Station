using Content.Goobstation.Shared.Terror.Components;
using Content.Goobstation.Shared.Terror.Events;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Terror.Systems;

/// <summary>
/// Handles egg-laying behavior for all terror spider variants.
/// Dispatches role-specific logic (Green, Queen, Princess)
/// to dedicated handlers.
/// </summary>
public sealed class TerrorLaySystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TerrorLayComponent, TerrorLayEvent>(OnLay);
    }

    private void OnLay(Entity<TerrorLayComponent> spider, ref TerrorLayEvent args)
    {
        var uid = spider.Owner;
        var layComp = spider.Comp;

        if (HasComp<GreenTerrorComponent>(uid))
        {
            args.Handled = HandleGreenLay(uid, layComp, args.Target);
            return;
        }

        if (TryComp<TerrorQueenComponent>(uid, out var queen))
        {
            args.Handled = HandleQueenLay(uid, layComp, queen, args.Target);
            return;
        }

        if (TryComp<PrincessTerrorComponent>(uid, out var princess))
        {
            args.Handled = HandlePrincessLay(uid, layComp, princess, args.Target);
            return;
        }
    }

    private bool HandleGreenLay(EntityUid uid, TerrorLayComponent layComp, EntityUid target)
    {
        // Green Terror: only lays on cocoons, always Tier 1
        if (!HasComp<CocoonComponent>(target))
        {
            _popup.PopupClient(
                Loc.GetString("terror-lay-only-cocoon"),
                uid,
                uid);
            return false;
        }

        return TryLayFromList(layComp.EggsTier1, target);
    }

    // Queen egg tiers scale with hive growth using diminishing returns.
    // Chances interpolate from base -> max as total wraps increase.
    private bool HandleQueenLay(
    EntityUid uid,
    TerrorLayComponent layComp,
    TerrorQueenComponent queen,
    EntityUid target)
    {
        var wraps = queen.HiveTotalWrappedAmount;

        var t2Chance = DiminishingChance(wraps, queen.Tier2BaseChance, queen.Tier2MaxChance, queen.Tier2CurveK);
        var t3Chance = DiminishingChance(wraps, queen.Tier3BaseChance, queen.Tier3MaxChance, queen.Tier3CurveK);

        // Clamp to ensure total probability never exceeds 1.0
        t3Chance = MathF.Min(t3Chance, 1f);
        t2Chance = MathF.Min(t2Chance, 1f - t3Chance);

        var roll = _random.NextFloat();

        if (roll < t3Chance)
            return TryLayFromList(layComp.EggsTier3, target);

        if (roll < t3Chance + t2Chance)
            return TryLayFromList(layComp.EggsTier2, target);

        return TryLayFromList(layComp.EggsTier1, target);
    }
    private bool HandlePrincessLay(
    EntityUid uid,
    TerrorLayComponent layComp,
    PrincessTerrorComponent princess,
    EntityUid target)
    {
        var t2Chance = princess.Tier2EggChance;
        var t3Chance = princess.Tier3EggChance;

        t3Chance = MathF.Min(t3Chance, 1f);
        t2Chance = MathF.Min(t2Chance, 1f - t3Chance);

        var roll = _random.NextFloat();

        if (roll < t3Chance)
            return TryLayFromList(layComp.EggsTier3, target);

        if (roll < t3Chance + t2Chance)
            return TryLayFromList(layComp.EggsTier2, target);

        return TryLayFromList(layComp.EggsTier1, target);
    }

    private static float DiminishingChance(int wrapped, float baseChance, float maxChance, float k)
    {
        // If provided base >= max, return base (or clamped max)
        if (baseChance >= maxChance)
            return MathF.Min(baseChance, 1f);

        // Returns a smooth diminishing-return interpolation from baseChance to maxChance
        // using an exponential decay curve controlled by k.
        var scale = 1f - MathF.Exp(-wrapped / k);
        return baseChance + (maxChance - baseChance) * scale;
    }

    private bool TryLayFromList(List<EntProtoId> list, EntityUid at)
    {
        if (list.Count == 0)
            return false;

        var protoId = list[_random.Next(list.Count)];
        var entity = Spawn(protoId, Transform(at).Coordinates);

        return entity != EntityUid.Invalid;
    }
}
