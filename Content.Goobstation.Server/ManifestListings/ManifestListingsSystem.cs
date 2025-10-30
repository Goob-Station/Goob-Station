using System.Linq;
using System.Text;
using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.ManifestListings;
using Content.Shared.Actions.Components;
using Content.Shared.Mind;
using Content.Shared.Store;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.ManifestListings;

public sealed class ManifestListingsSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MindComponent, ListingPurchasedEvent>(OnPurchase);

        SubscribeLocalEvent<MindListingsComponent, PrependObjectivesSummaryTextEvent>(OnPrepend);
    }

    private void OnPurchase(Entity<MindComponent> ent, ref ListingPurchasedEvent args)
    {
        var listings = EnsureComp<MindListingsComponent>(ent);

        if (!listings.Listings.TryGetValue(args.Store.Id, out var list))
        {
            list = new();
            listings.Listings.Add(args.Store.Id, list);
        }

        var data = args.Data;
        list.RemoveAll(x => x.ID == data.ID);
        list.Add(data);
    }

    private void OnPrepend(Entity<MindListingsComponent> ent, ref PrependObjectivesSummaryTextEvent args)
    {
        var sb = new StringBuilder();
        var sb2 = new StringBuilder();

        Dictionary<ProtoId<CurrencyPrototype>, FixedPoint2> totalSpent = new();
        foreach (var list in ent.Comp.Listings.Values)
        {
            var storeSb = new StringBuilder();
            HashSet<string> ignoredIds = new();
            // Data id -> amount purchased (needed for action upgrades)
            Dictionary<string, int> info = new();
            foreach (var data in list)
            {
                if (data.PurchaseAmount <= 0)
                    continue;

                if (!info.TryAdd(data.ID, data.PurchaseAmount))
                    info[data.ID] += data.PurchaseAmount;

                if (data.ProductUpgradeId == null)
                    continue;

                ignoredIds.Add(data.ProductUpgradeId);
                var upgrade = list.FirstOrDefault(x => x.ID == data.ProductUpgradeId);
                if (upgrade != null)
                {
                    info[data.ID] += upgrade.PurchaseAmount;
                }
            }

            foreach (var (dataId, count) in info)
            {
                if (ignoredIds.Contains(dataId))
                    continue;

                var data = list.FirstOrDefault(x => x.ID == dataId);
                if (data == null)
                    continue;

                Dictionary<ProtoId<CurrencyPrototype>, FixedPoint2> cost = new();
                var costMultiplier = count;

                if (data.SaleCost != null)
                {
                    var salePurchases = Math.Min(data.SaleLimit, count);
                    foreach (var (currency, amount) in data.SaleCost)
                    {
                        if (!cost.TryAdd(currency, amount * salePurchases))
                            cost[currency] += amount * salePurchases;
                    }

                    costMultiplier -= salePurchases;
                }

                if (costMultiplier != 0)
                {
                    foreach (var (currency, amount) in data.Cost)
                    {
                        if (!cost.TryAdd(currency, amount * costMultiplier))
                            cost[currency] += amount * costMultiplier;
                    }
                }

                string sprite;
                var state = "";
                switch (data.Icon)
                {
                    case SpriteSpecifier.Texture tex:
                    {
                        sprite = tex.TexturePath.ToString();
                        if (!sprite.StartsWith("/Textures/"))
                            sprite = $"/Textures/{sprite}";
                        break;
                    }
                    case SpriteSpecifier.Rsi rsi:
                        sprite = rsi.RsiPath.ToString();
                        state = rsi.RsiState;
                        break;
                    default:
                    {
                        if (data.ProductEntity != null)
                            sprite = data.ProductEntity.Value;
                        else if (data.ProductAction != null && TryGetActionIcon(data.ProductAction.Value,
                                     out var actionSprite,
                                     out var actionState))
                        {
                            sprite = actionSprite;
                            state = actionState;
                        }
                        else
                            sprite = ent.Comp.DefaultTexture.TexturePath.ToString();

                        break;
                    }
                }

                var name = "";
                if (data.Name != null)
                    name = Loc.GetString(data.Name);
                else
                {
                    if (data.ProductEntity != null)
                        name = Loc.GetString(_proto.Index(data.ProductEntity.Value).Name);
                    else if (data.ProductAction != null)
                        name = Loc.GetString(_proto.Index(data.ProductAction.Value).Name);
                }

                var costSb = new StringBuilder();
                foreach (var (currencyId, amount) in cost)
                {
                    if (!totalSpent.TryAdd(currencyId, amount))
                        totalSpent[currencyId] += amount;

                    if (costSb.Length > 0)
                        costSb.Append(", ");

                    var currency = _proto.Index(currencyId);
                    costSb.Append($"{amount} {Loc.GetString(currency.DisplayName)}");
                }

                var information = Loc.GetString("manifest-listing-entry-info",
                    ("name", name),
                    ("spent", costSb.ToString()));

                information = information.Replace("\"", ""); // Fuck this
                information = information.Replace("\'", ""); // Fuck this

                storeSb.Append(Loc.GetString("manifest-listing-entry-listing",
                    ("sprite", sprite),
                    ("state", state),
                    ("info", information),
                    ("amount", count)));
            }

            sb2.Append(storeSb.ToString());
        }

        var totalSpentSb = new StringBuilder();
        foreach (var (currencyId, amount) in totalSpent)
        {
            if (totalSpentSb.Length > 0)
                totalSpentSb.Append(", ");

            var currency = _proto.Index(currencyId);
            totalSpentSb.Append($"{amount} {Loc.GetString(currency.DisplayName)}");
        }

        sb.AppendLine(Loc.GetString("manifest-listing-entry-start", ("spent", totalSpentSb.ToString())));
        sb.AppendLine();
        sb.AppendLine(sb2.ToString());
        args.Text += sb.ToString();
    }

    private bool TryGetActionIcon(EntProtoId proto, out string sprite, out string state)
    {
        sprite = "";
        state = "";

        if (!_proto.Index(proto).TryGetComponent("Action", out ActionComponent? actionComp) || actionComp.Icon == null)
            return false;

        switch (actionComp.Icon)
        {
            case SpriteSpecifier.Texture tex:
            {
                sprite = tex.TexturePath.ToString();
                if (!sprite.StartsWith("/Textures/"))
                    sprite = $"/Textures/{sprite}";
                return true;
            }
            case SpriteSpecifier.Rsi rsi:
                sprite = rsi.RsiPath.ToString();
                state = rsi.RsiState;
                return true;
            default:
                return false;
        }
    }
}
