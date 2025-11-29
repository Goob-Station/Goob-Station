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
        var layComp = spider.Comp; // avoid repeated spider.Comp access and nullable warnings

        // If the spider is "Green Terror", they can ONLY lay on cocoons
        if (TryComp<GreenTerrorComponent>(uid, out _))
        {
            if (!HasComp<CocoonComponent>(args.Target))
            {
                _popup.PopupClient("You can only lay eggs on cocoons.", uid, uid);
                return;
            }

            // Green always produces Tier 1 spiderling eggs
            if (TryLayFromList(layComp.EggsTier1, args.Target))
                args.Handled = true;

            return;
        }

        // === Non-green spiders follow the chance system ===

        // Case 1: Queen
        if (TryComp<TerrorQueenComponent>(uid, out var queen))
        {
            var t2Chance = queen.Tier2EggChance * (1 + queen.HiveTotalWrappedAmount);
            var t3Chance = queen.Tier3EggChance * (1 + queen.HiveTotalWrappedAmount);

            var roll = _random.NextFloat();

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

            // Tier 1
            if (TryLayFromList(spider.Comp.EggsTier1, args.Target))
                args.Handled = true;

            return;
        }

        // Case 2: Princess
        if (TryComp<PrincessTerrorComponent>(uid, out var princess))
        {
            var t2Chance = princess.Tier2EggChance;
            var t3Chance = princess.Tier3EggChance;

            var roll = _random.NextFloat();

            // Tier 3
            if (roll < t3Chance)
            {
                if (TryLayFromList(layComp.EggsTier3, args.Target))
                    args.Handled = true;
                return;
            }

            // Tier 2
            if (roll < t2Chance + t3Chance)
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

        // Case 3: Neither queen nor princess found
        // Do nothing
        return;

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
