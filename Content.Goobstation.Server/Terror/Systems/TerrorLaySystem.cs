using Content.Goobstation.Shared.Terror.Components;
using Content.Goobstation.Shared.Terror.Events;
using Content.Goobstation.Shared.Terror.Gamerules;
using Content.Shared.GameTicking.Components;
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
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TerrorSpiderComponent, TerrorLayEvent>(OnLay);
    }

    private void OnLay(EntityUid uid, TerrorSpiderComponent comp, ref TerrorLayEvent args)
    {
        if (!_proto.TryIndex(comp.SpiderType, out var proto))
            return;

        if (!proto.CanLay || proto.LayConfig == null)
            return;

        var lay = proto.LayConfig;

        if (lay.Tiers.Count == 0)
            return;

        int wraps = 0;

        if (HasComp<TerrorQueenComponent>(uid))
        {
            var rules = EntityQueryEnumerator<TerrorHiveRuleComponent, GameRuleComponent>();
            while (rules.MoveNext(out var _, out var rule, out _))
            {
                wraps = rule.TotalWrapped;
                break;
            }
        }

        var roll = _random.NextFloat();
        var cumulative = 0f;
        var selected = false;

        foreach (var tier in lay.Tiers)
        {
            var chance = tier.BaseChance;

            if (tier.ScaleWithHive &&
                tier.MaxChance is { } max &&
                tier.CurveK is { } k)
            {
                chance = DiminishingChance(wraps, tier.BaseChance, max, k);
            }

            cumulative += chance;

            if (roll < cumulative)
            {
                TryRandomSpawnFromList(tier.Prototypes, args.Target);
                selected = true;
                break;
            }
        }

        // Fallback
        if (!selected)
        {
            var fallbackTier = lay.Tiers[0];

            TryRandomSpawnFromList(fallbackTier.Prototypes, args.Target);
        }

        args.Handled = true;
    }

    private void TryRandomSpawnFromList(List<EntProtoId> list, EntityUid at)
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
