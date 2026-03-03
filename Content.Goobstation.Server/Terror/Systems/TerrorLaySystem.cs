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
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TerrorSpiderComponent, TerrorLayEvent>(OnLay);
    }

    private void OnLay(EntityUid uid, TerrorSpiderComponent comp, ref TerrorLayEvent args)
    {
        var proto = _proto.Index(comp.SpiderType);

        if (!proto.CanLay || proto.LayConfig is null)
            return;

        var lay = proto.LayConfig;

        float tier2Chance = lay.Tier2Chance;
        float tier3Chance = lay.Tier3Chance;
        float tier4Chance = lay.Tier4Chance;

        if (proto.HiveScaling is { } scaling &&
            TryComp<TerrorQueenComponent>(uid, out var queen))
        {
            var wraps = queen.HiveTotalWrappedAmount;

            tier2Chance = DiminishingChance(
                wraps,
                scaling.Tier2BaseChance,
                scaling.Tier2MaxChance,
                scaling.Tier2CurveK);

            tier3Chance = DiminishingChance(
                wraps,
                scaling.Tier3BaseChance,
                scaling.Tier3MaxChance,
                scaling.Tier3CurveK);
        }
        // else basically do the regular stuff and yadda yadda

        var roll = _random.NextFloat();
        var cumulative = 0f;

        cumulative += tier4Chance;
        if (roll < cumulative)
        {
            TrySpawnFromList(lay.Tier4, args.Target);
            args.Handled = true;
            return;
        }

        cumulative += tier3Chance;
        if (roll < cumulative)
        {
            TrySpawnFromList(lay.Tier3, args.Target);
            args.Handled = true;
            return;
        }

        cumulative += tier2Chance;
        if (roll < cumulative)
        {
            TrySpawnFromList(lay.Tier2, args.Target);
            args.Handled = true;
            return;
        }

        TrySpawnFromList(lay.Tier1, args.Target);
        args.Handled = true;
    }
    private void TrySpawnFromList(List<EntProtoId> list, EntityUid at)
    {
        if (list.Count == 0)
            return;

        var protoId = list[_random.Next(list.Count)];
        Spawn(protoId, Transform(at).Coordinates);
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
}
