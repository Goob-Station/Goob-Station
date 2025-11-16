using Content.Goobstation.Server.Pirates.GameTicking.Rules;
using Content.Server.Cargo.Components;
using Content.Server.Cargo.Systems;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Shared.Cargo.Components;
using Content.Shared.Cargo.Events;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mobs.Components;

namespace Content.Goobstation.Server.Pirates;

public sealed class PiratesPalletConsoleSystem : EntitySystem
{

    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly PricingSystem _pricing = default!;

    private EntityQuery<CargoSellBlacklistComponent> _blacklistQuery;
    private List<(EntityUid, CargoPalletComponent, TransformComponent)> _pads = new();
    private HashSet<EntityUid> _setEnts = new();

    public override void Initialize()
    {
        base.Initialize();

        _blacklistQuery = GetEntityQuery<CargoSellBlacklistComponent>();
        SubscribeLocalEvent<PiratesPalletConsoleComponent, CargoPalletSellMessage>(OnPalletSale);
    }

    private void OnPalletSale(EntityUid uid, PiratesPalletConsoleComponent component, CargoPalletSellMessage args)
    {
        if (!_mind.TryGetMind(args.Actor, out var mindId,out var mind))
            return;

        var query = EntityQueryEnumerator<ActiveGameRuleComponent, ActivePirateRuleComponent>();
        while (query.MoveNext(out var quaryUid, out var _ ,out var piratesRule))
        {
            if (!piratesRule.Pirates.Contains((mindId, mind)))
                continue;

            var grid = Transform(uid).GridUid;
            if(grid == null)
                continue;

            if (!SellPallets(grid.Value, out var goods))
               continue;

            var total = (double) 0;
            foreach (var (_, _,value ) in goods)
                total += value;

            piratesRule.Credits += (float) Math.Round(total);

            return;
        }
        _popup.PopupEntity("cant sell", args.Actor,args.Actor);
    }

    private bool SellPallets(EntityUid gridUid , out HashSet<(EntityUid, OverrideSellComponent?, double)> goods)
    {
        GetPalletGoods(gridUid, out var toSell, out goods);

        if (toSell.Count == 0)
            return false;

        //var ev = new EntitySoldEvent(toSell, station);
        //RaiseLocalEvent(ref ev);

        foreach (var ent in toSell)
            QueueDel(ent);

        return true;
    }

    private void GetPalletGoods(EntityUid gridUid, out HashSet<EntityUid> toSell,  out HashSet<(EntityUid, OverrideSellComponent?, double)> goods)
    {
        goods = new HashSet<(EntityUid, OverrideSellComponent?, double)>();
        toSell = new HashSet<EntityUid>();

        foreach (var (palletUid, _, _) in GetCargoPallets(gridUid, BuySellType.Sell))
        {
            // Containers should already get the sell price of their children so can skip those.
            _setEnts.Clear();

            _lookup.GetEntitiesIntersecting(
                palletUid,
                _setEnts,
                LookupFlags.Dynamic | LookupFlags.Sundries);

            foreach (var ent in _setEnts)
            {
                // Dont sell:
                // - anything already being sold
                // - anything anchored (e.g. light fixtures)
                // - anything blacklisted (e.g. players).
                if (toSell.Contains(ent) &&
                    (Transform(ent).Anchored || !CanSell(ent, Transform(ent))))
                {
                    continue;
                }

                if (_blacklistQuery.HasComponent(ent))
                    continue;

                var price = _pricing.GetPrice(ent);
                if (price == 0)
                    continue;
                toSell.Add(ent);
                goods.Add((ent, CompOrNull<OverrideSellComponent>(ent), price));
            }
        }
    }

    private List<(EntityUid Entity, CargoPalletComponent Component, TransformComponent PalletXform)> GetCargoPallets(EntityUid gridUid, BuySellType requestType = BuySellType.All)
    {
        _pads.Clear();

        var query = AllEntityQuery<CargoPalletComponent, TransformComponent>();

        while (query.MoveNext(out var uid, out var comp, out var compXform))
        {
            if (compXform.ParentUid != gridUid ||
                !compXform.Anchored)
            {
                continue;
            }

            if ((requestType & comp.PalletType) == 0)
            {
                continue;
            }

            _pads.Add((uid, comp, compXform));

        }

        return _pads;
    }

    private bool CanSell(EntityUid uid, TransformComponent xform)
    {
        if (HasComp<MobStateComponent>(uid))
            return false;

        // Recursively check for mobs at any point.
        var children = xform.ChildEnumerator;
        while (children.MoveNext(out var child))
        {

            if (!CanSell(child, Transform(child)))
                return false;
        }

        return true;
    }
}
