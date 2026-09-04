using Content.Goobstation.Shared.Terror.Components;
using Content.Goobstation.Shared.Terror.Events;
using Content.Goobstation.Shared.Terror.Gamerules;
using Content.Goobstation.Shared.Terror.Systems;
using Content.Shared._Starlight.VentCrawling;
using Content.Shared.GameTicking.Components;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Terror.Systems;

/// <summary>
/// Use up stored egg, roll the dice on what tier of spider to spawn, then, well, spawn the result.
/// </summary>
public sealed class TerrorLaySystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SpiderEggLayerSystem _eggLayer = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TerrorSpiderComponent, TerrorLayEvent>(OnLay);
    }

    private void OnLay(EntityUid uid, TerrorSpiderComponent comp, ref TerrorLayEvent args)
    {
        if (!_proto.TryIndex(comp.SpiderType, out var proto))
            return;

        if (!proto.IsEggLayer || proto.LayConfig is not { } lay)
            return;

        if (lay.Tiers.Count == 0)
            return;

        if (!_eggLayer.TryConsumeEgg(uid))
            return;

        // Parity or something idk, makes it less annoying ig to corner her
        if (proto.IsQueen && HasComp<VentCrawlerComponent>(uid))
        {
            RemCompDeferred<VentCrawlerComponent>(uid);
            _popup.PopupPredicted(Loc.GetString("terror-queen-ventcrawl-gone"), uid, uid, PopupType.MediumCaution);
        }

        var wraps = GetHiveWrapCount();
        var roll = _random.NextFloat();
        var cumulative = 0f;

        foreach (var tier in lay.Tiers)
        {
            var chance = tier.BaseChance;

            if (tier.ScaleWithHive && tier.MaxChance is { } max && tier.CurveK is { } k)
            {
                chance = DiminishingChance(wraps, tier.BaseChance, max, k);
            }

            cumulative += chance;

            if (roll < cumulative)
            {
                TryRandomSpawnFromList(tier.Eggs, args.Target);
                args.Handled = true;
                return;
            }
        }

        // Rolls didn't add up to 1, so fall back to the first tier rather than waste the egg.
        TryRandomSpawnFromList(lay.Tiers[0].Eggs, args.Target);
        args.Handled = true;
    }

    private int GetHiveWrapCount()
    {
        var rules = EntityQueryEnumerator<TerrorHiveRuleComponent, GameRuleComponent>();

        while (rules.MoveNext(out _, out var rule, out _))
            return rule.TotalWrapped;

        return 0;
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
        if (baseChance >= maxChance)
            return MathF.Min(baseChance, 1f);

        var scale = 1f - MathF.Exp(-wrapped / k);
        return baseChance + (maxChance - baseChance) * scale;
    }
}
