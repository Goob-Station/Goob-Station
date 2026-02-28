using Content.Goobstation.Shared.Terror.Components;
using Content.Goobstation.Shared.Terror.Events;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System;

namespace Content.Goobstation.Server.Terror.Systems;

public sealed class TerrorLaySystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TerrorLayComponent, TerrorLayEvent>(OnLay);
    }

    private void OnLay(Entity<TerrorLayComponent> spider, ref TerrorLayEvent args)
    {
        var uid = spider.Owner;
        var layComp = spider.Comp;

        // === Green Terror: Only on cocoons, always Tier 1 ===
        if (TryComp<GreenTerrorComponent>(uid, out _))
        {
            if (!HasComp<CocoonComponent>(args.Target))
            {
                _popup.PopupClient(
                    Loc.GetString("terror-lay-only-cocoon"),
                    uid,
                    uid);
                return;
            }

            if (TryLayFromList(layComp.EggsTier1, args.Target))
                args.Handled = true;

            return;
        }

        // === Queen: Diminishing returns, capped curve ===
        if (TryComp<TerrorQueenComponent>(uid, out var queen))
        {
            var wraps = queen.HiveTotalWrappedAmount;

            // Compute diminishing-return chances that interpolate from base -> max
            var t2Chance = DiminishingChance(wraps, queen.Tier2BaseChance, queen.Tier2MaxChance, queen.Tier2CurveK);
            var t3Chance = DiminishingChance(wraps, queen.Tier3BaseChance, queen.Tier3MaxChance, queen.Tier3CurveK);

            // Safety clamp: t3 cannot exceed 1, and t2 cannot push total above 1
            t3Chance = MathF.Min(t3Chance, 1f);
            t2Chance = MathF.Min(t2Chance, 1f - t3Chance);

            var roll = _random.NextFloat();

            // Tier 3
            if (roll < t3Chance)
            {
                if (TryLayFromList(layComp.EggsTier3, args.Target))
                    args.Handled = true;
                return;
            }

            // Tier 2
            if (roll < t3Chance + t2Chance)
            {
                if (TryLayFromList(layComp.EggsTier2, args.Target))
                    args.Handled = true;
                return;
            }

            // Tier 1
            if (TryLayFromList(layComp.EggsTier1, args.Target))
                args.Handled = true;

            return;
        }

        // === Princess: Flat chances, no diminishing returns ===
        if (TryComp<PrincessTerrorComponent>(uid, out var princess))
        {
            var t2Chance = princess.Tier2EggChance;
            var t3Chance = princess.Tier3EggChance;

            // Safety clamp for princess in case values are weird
            t3Chance = MathF.Min(t3Chance, 1f);
            t2Chance = MathF.Min(t2Chance, 1f - t3Chance);

            var roll = _random.NextFloat();

            if (roll < t3Chance)
            {
                if (TryLayFromList(layComp.EggsTier3, args.Target))
                    args.Handled = true;
                return;
            }

            if (roll < t3Chance + t2Chance)
            {
                if (TryLayFromList(layComp.EggsTier2, args.Target))
                    args.Handled = true;
                return;
            }

            if (TryLayFromList(layComp.EggsTier1, args.Target))
                args.Handled = true;

            return;
        }

        // Neither queen nor princess: do nothing
        return;
    }

    private static float DiminishingChance(int wrapped, float baseChance, float maxChance, float k)
    {
        // If provided base >= max, return base (or clamped max)
        if (baseChance >= maxChance)
            return MathF.Min(baseChance, 1f);

        // Smooth interpolation from base -> max: base + (max-base) * (1 - exp(-wrapped/k))
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
