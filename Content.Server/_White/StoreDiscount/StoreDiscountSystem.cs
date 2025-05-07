// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.FixedPoint;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Robust.Shared.Random;

namespace Content.Server._White.StoreDiscount;

public sealed class StoreDiscountSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    public void ApplyDiscounts(IEnumerable<ListingData> listings, StoreComponent store)
    {
        if (!store.Sales.Enabled)
            return;

        var count = _random.Next(store.Sales.MinItems, store.Sales.MaxItems + 1);

        listings = listings
            .Where(l => !l.SaleBlacklist && l.Cost.Any(x => x.Value > 1) && store.Categories.Overlaps(l.Categories)) // goob edit
            .OrderBy(_ => _random.Next()).Take(count).ToList();

        foreach (var listing in listings)
        {
            var sale = GetDiscount(store.Sales.MinMultiplier, store.Sales.MaxMultiplier);
            var newCost = listing.Cost.ToDictionary(x => x.Key,
                x => FixedPoint2.New(Math.Max(1, (int) MathF.Round(x.Value.Float() * sale))));

            if (listing.Cost.All(x => x.Value.Int() == newCost[x.Key].Int()))
                continue;

            var key = listing.Cost.First(x => x.Value > 0).Key;
            listing.OldCost = listing.Cost;
            listing.DiscountValue = 100 - (newCost[key] / listing.Cost[key] * 100).Int();
            listing.Cost = newCost;
            listing.Categories = new() { store.Sales.SalesCategory };
        }
    }

    private float GetDiscount(float minMultiplier, float maxMultiplier)
    {
        return _random.NextFloat() * (maxMultiplier - minMultiplier) + minMultiplier;
    }
}