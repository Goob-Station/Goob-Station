// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Armok <155400926+ARMOKS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 IrisTheAmped <iristheamped@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Server.NukeOps.NukieBundle;
using Content.Server.Storage.EntitySystems;
using Content.Server.Store.Systems;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Robust.Shared.Random;


namespace Content.Goobstation.Server.Nukeops;

public sealed class NukieSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EntityStorageSystem _entityStorage = default!;
    [Dependency] private readonly StoreSystem _store = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NukieBundleComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, NukieBundleComponent component, MapInitEvent args)
    {
        if (!TryComp<StoreComponent>(uid, out var store))
            return;

        FillStorage((uid, component, store));
    }

    private void FillStorage(Entity<NukieBundleComponent, StoreComponent> ent)
    {
        var cords = Transform(ent).Coordinates;
        var content = GetRandomContent(ent);
        foreach (var item in content)
        {
            var dode = Spawn(item.ProductEntity, cords);
            _entityStorage.Insert(dode, ent);
        }
    }

    private List<ListingData> GetRandomContent(Entity<NukieBundleComponent, StoreComponent> ent)
    {
        var ret = new List<ListingData>();

        var listings = _store.GetAvailableListings(ent.Owner, ent.Owner, ent.Comp2)   // Basically turns the Bundle into the buyer, which allows it's tag to filter
            .OrderBy(p => p.Cost.Values.Sum())
            .ToList();

        if (listings.Count == 0)
            return ret;

        var totalCost = FixedPoint2.Zero;
        var index = 0;
        while (totalCost < ent.Comp1.TotalPrice)
        {
            var remainingBudget = ent.Comp1.TotalPrice - totalCost;
            while (listings[index].Cost.Values.Sum() > remainingBudget)
            {
                index++;
                if (index >= listings.Count)
                {

                    return ret;
                }
            }
            var randomIndex = _random.Next(index, listings.Count);
            var randomItem = listings[randomIndex];
            ret.Add(randomItem);
            totalCost += randomItem.Cost.Values.Sum();
        }

        return ret;
    }
}
