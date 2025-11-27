using Content.Goobstation.Shared.Terror.Components;
using Content.Goobstation.Shared.Terror.Events;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

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

        // If the spider is "Green Terror", they can ONLY lay on cocoons
        if (TryComp<GreenTerrorComponent>(uid, out _))
        {
            if (!HasComp<CocoonComponent>(args.Target))
            {
                _popup.PopupClient("You can only lay eggs inside cocoons.", uid, uid);
                return;
            }

            // Green always produces Tier 1 spiderling eggs
            if (TryLayFromList(spider.Comp.EggsTier1, args.Target))
                args.Handled = true;

            return;
        }

        // === Non-green spiders follow the chance system ===

        if (!TryComp<TerrorQueenComponent>(uid, out var queen))
        {
            _popup.PopupClient("No queen component found.", uid, uid);
            return;
        }

        var t2Chance = queen.Tier2EggChance * (1 + queen.HiveTotalWrappedAmount);
        var t3Chance = queen.Tier3EggChance * (1 + queen.HiveTotalWrappedAmount);

        var roll = _random.NextFloat(); // 0.0 to 1.0

        // Tier 3 check
        if (roll < t3Chance)
        {
            if (TryLayFromList(spider.Comp.EggsTier3, args.Target))
                args.Handled = true;

            return;
        }

        // Tier 2 check
        if (roll < t2Chance + t3Chance)
        {
            if (TryLayFromList(spider.Comp.EggsTier2, args.Target))
                args.Handled = true;

            return;
        }

        // Otherwise Tier 1
        if (TryLayFromList(spider.Comp.EggsTier1, args.Target))
            args.Handled = true;
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
