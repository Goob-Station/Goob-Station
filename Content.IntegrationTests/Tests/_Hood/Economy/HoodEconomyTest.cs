// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server.Stack;
using Content.Server.Store.Components;
using Content.Server.Store.Systems;
using Content.Shared.Stacks;
using Content.Shared.Storage;
using Content.Shared.Storage.EntitySystems;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Content.Shared.Tag;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests._Hood.Economy;

[TestFixture]
public sealed class HoodEconomyTest
{
    [TestPrototypes]
    private const string TestPrototypes = """
- type: entity
  id: HoodRetailStoreTestEntity
  parent: StorePresetHoodRetail
""";

    private static readonly (string Entity, int Value, string Stack)[] Denominations =
    {
        ("HoodCash1", 1, "HoodDollar1"),
        ("HoodCash5", 5, "HoodDollar5"),
        ("HoodCash10", 10, "HoodDollar10"),
        ("HoodCash20", 20, "HoodDollar20"),
        ("HoodCash50", 50, "HoodDollar50"),
        ("HoodCash100", 100, "HoodDollar100"),
    };

    private static readonly string[] RetailListings =
    {
        "HoodRetailWaterBottle",
        "HoodRetailChips",
        "HoodRetailLighter",
    };

    [Test]
    public async Task CashStacksRegisterAndRetailCatalogUseOnlyHoodDollars()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var testMap = await pair.CreateTestMap();
        var entities = server.ResolveDependency<IEntityManager>();
        var prototypes = server.ResolveDependency<IPrototypeManager>();
        var stackSystem = entities.System<StackSystem>();
        var storageSystem = entities.System<SharedStorageSystem>();
        var storeSystem = entities.System<StoreSystem>();
        var tagSystem = entities.System<TagSystem>();

        EntityUid register = default;
        EntityUid storedCash = default;
        EntityUid unrelated = default;

        await server.WaitAssertion(() =>
        {
            var currency = prototypes.Index<CurrencyPrototype>("HoodDollar");
            Assert.Multiple(() =>
            {
                Assert.That(currency.CanWithdraw, Is.True);
                Assert.That(currency.Cash, Is.Not.Null);
                Assert.That(currency.Cash!.Select(entry => (entry.Key.Int(), entry.Value.Id)),
                    Is.EquivalentTo(Denominations.Select(entry => (entry.Value, entry.Entity))));
            });

            foreach (var denomination in Denominations)
            {
                var cash = entities.SpawnEntity(denomination.Entity, testMap.GridCoords);
                var stack = entities.GetComponent<StackComponent>(cash);
                var value = entities.GetComponent<CurrencyComponent>(cash);

                Assert.Multiple(() =>
                {
                    Assert.That(stack.Count, Is.EqualTo(1), denomination.Entity);
                    Assert.That(stack.StackTypeId.Id, Is.EqualTo(denomination.Stack), denomination.Entity);
                    Assert.That(stackSystem.GetMaxCount(stack), Is.EqualTo(100), denomination.Entity);
                    Assert.That(value.Price.Keys, Is.EquivalentTo(new[] { "HoodDollar" }), denomination.Entity);
                    Assert.That(value.Price["HoodDollar"].Int(), Is.EqualTo(denomination.Value), denomination.Entity);
                    Assert.That(storeSystem.GetCurrencyValue(cash, value)["HoodDollar"].Int(),
                        Is.EqualTo(denomination.Value), denomination.Entity);
                    Assert.That(tagSystem.HasTag(cash, "HoodCash"), Is.True, denomination.Entity);
                });
            }

            var stackCash = entities.SpawnEntity("HoodCash20", testMap.GridCoords);
            var originalStack = entities.GetComponent<StackComponent>(stackCash);
            stackSystem.SetCount((stackCash, originalStack), 3);

            var splitCash = stackSystem.Split((stackCash, originalStack), 1, testMap.GridCoords);
            Assert.That(splitCash, Is.Not.Null);
            var splitStack = entities.GetComponent<StackComponent>(splitCash!.Value);

            Assert.Multiple(() =>
            {
                Assert.That(originalStack.Count, Is.EqualTo(2));
                Assert.That(splitStack.Count, Is.EqualTo(1));
                Assert.That(splitStack.StackTypeId, Is.EqualTo(originalStack.StackTypeId));
                Assert.That(storeSystem.GetCurrencyValue(stackCash,
                    entities.GetComponent<CurrencyComponent>(stackCash))["HoodDollar"].Int(), Is.EqualTo(40));
            });

            Assert.That(stackSystem.TryMergeStacks(
                (splitCash.Value, splitStack),
                (stackCash, originalStack),
                out var transferred), Is.True);
            Assert.Multiple(() =>
            {
                Assert.That(transferred, Is.EqualTo(1));
                Assert.That(originalStack.Count, Is.EqualTo(3));
                Assert.That(storeSystem.GetCurrencyValue(stackCash,
                    entities.GetComponent<CurrencyComponent>(stackCash))["HoodDollar"].Int(), Is.EqualTo(60));
            });

            register = entities.SpawnEntity("HoodCashRegister", testMap.GridCoords);
            storedCash = entities.SpawnEntity("HoodCash50", testMap.GridCoords);
            unrelated = entities.SpawnEntity("Paper", testMap.GridCoords);
            var storedStack = entities.GetComponent<StackComponent>(storedCash);
            stackSystem.SetCount((storedCash, storedStack), 3);

            Assert.Multiple(() =>
            {
                Assert.That(storageSystem.CanInsert(register, storedCash, out _), Is.True);
                Assert.That(storageSystem.CanInsert(register, unrelated, out _), Is.False);
                Assert.That(storageSystem.Insert(register, storedCash, out _), Is.True);
            });

            Assert.That(prototypes.HasIndex<EntityPrototype>("StorePresetHoodRetail"), Is.False,
                "Abstract entity prototypes are intentionally not indexable; the retail preset must remain opt-in.");

            var retailStore = entities.SpawnEntity("HoodRetailStoreTestEntity", testMap.GridCoords);
            var store = entities.GetComponent<StoreComponent>(retailStore);
            Assert.Multiple(() =>
            {
                Assert.That(store.CurrencyWhitelist.Select(id => id.Id), Is.EquivalentTo(new[] { "HoodDollar" }));
                Assert.That(store.Balance.Keys.Select(id => id.Id), Is.EquivalentTo(new[] { "HoodDollar" }));
                Assert.That(store.Categories.Select(id => id.Id), Is.EquivalentTo(new[] { "HoodRetailBasics" }));
                Assert.That(storeSystem.GetAvailableListings(retailStore, retailStore, store)
                    .Select(listing => listing.ID), Is.EquivalentTo(RetailListings));
            });

            foreach (var listingId in RetailListings)
            {
                var listing = prototypes.Index<ListingPrototype>(listingId);
                Assert.Multiple(() =>
                {
                    Assert.That(listing.Categories.Select(id => id.Id),
                        Is.EquivalentTo(new[] { "HoodRetailBasics" }), listingId);
                    Assert.That(listing.Cost.Keys.Select(id => id.Id),
                        Is.EquivalentTo(new[] { "HoodDollar" }), listingId);
                    Assert.That(listing.Cost["HoodDollar"].Int(), Is.GreaterThan(0), listingId);
                    Assert.That(listing.ProductEntity, Is.Not.Null, listingId);
                    Assert.That(prototypes.HasIndex<EntityPrototype>(listing.ProductEntity!.Value), Is.True, listingId);
                });
            }

            var storeCash = entities.SpawnEntity("HoodCash100", testMap.GridCoords);
            var storeCashStack = entities.GetComponent<StackComponent>(storeCash);
            var storeCashValue = entities.GetComponent<CurrencyComponent>(storeCash);
            stackSystem.SetCount((storeCash, storeCashStack), 2);
            Assert.That(storeSystem.TryAddCurrency((storeCash, storeCashValue), (retailStore, store)), Is.True);
            Assert.That(store.Balance["HoodDollar"].Int(), Is.EqualTo(200));
        });

        await server.WaitRunTicks(3);

        EntityUid withdrawn = default;
        await server.WaitAssertion(() =>
        {
            var storage = entities.GetComponent<StorageComponent>(register);
            var storedStack = entities.GetComponent<StackComponent>(storedCash);
            Assert.Multiple(() =>
            {
                Assert.That(storage.Container.ContainedEntities, Does.Contain(storedCash));
                Assert.That(storage.StoredItems.Keys, Does.Contain(storedCash));
                Assert.That(storage.Container.ContainedEntities, Does.Not.Contain(unrelated));
                Assert.That(storedStack.Count, Is.EqualTo(3));
            });

            var additionalCash = entities.SpawnEntity("HoodCash50", testMap.GridCoords);
            var additionalStack = entities.GetComponent<StackComponent>(additionalCash);
            stackSystem.SetCount((additionalCash, additionalStack), 2);
            Assert.That(storageSystem.Insert(register, additionalCash, out var stackedEntity), Is.True);
            Assert.Multiple(() =>
            {
                Assert.That(stackedEntity, Is.EqualTo(storedCash));
                Assert.That(storedStack.Count, Is.EqualTo(5));
            });

            withdrawn = stackSystem.Split((storedCash, storedStack), 2, testMap.GridCoords)!.Value;
            Assert.Multiple(() =>
            {
                Assert.That(storedStack.Count, Is.EqualTo(3));
                Assert.That(entities.GetComponent<StackComponent>(withdrawn).Count, Is.EqualTo(2));
                Assert.That(storage.Container.ContainedEntities, Does.Contain(storedCash));
                Assert.That(storage.Container.ContainedEntities, Does.Not.Contain(withdrawn));
            });
        });

        await server.WaitRunTicks(3);
        await server.WaitAssertion(() =>
        {
            var storage = entities.GetComponent<StorageComponent>(register);
            Assert.Multiple(() =>
            {
                Assert.That(storage.Container.ContainedEntities, Does.Contain(storedCash));
                Assert.That(entities.GetComponent<StackComponent>(storedCash).Count, Is.EqualTo(3));
                Assert.That(entities.GetComponent<StackComponent>(withdrawn).Count, Is.EqualTo(2));
            });
        });

        await pair.CleanReturnAsync();
    }
}
