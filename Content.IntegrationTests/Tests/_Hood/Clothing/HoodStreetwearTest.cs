// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.Clothing.Components;
using Content.Shared.Store;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests._Hood.Clothing;

[TestFixture]
public sealed class HoodStreetwearTest
{
    private static readonly (string Entity, string Listing, string Category)[] Wardrobe =
    {
        ("HoodClothingUniformBaggyCream", "HoodClothingBaggyCream", "HoodRetailClothing"),
        ("HoodClothingUniformCargoGraphic", "HoodClothingCargoGraphic", "HoodRetailClothing"),
        ("HoodClothingOuterBlackZipHoodie", "HoodClothingBlackZipHoodie", "HoodRetailClothing"),
        ("HoodClothingOuterNavyVarsity", "HoodClothingNavyVarsity", "HoodRetailClothing"),
        ("HoodClothingShoesWhiteLowtops", "HoodClothingWhiteLowtops", "HoodRetailClothing"),
        ("HoodClothingHeadCharcoalFittedCap", "HoodClothingCharcoalFittedCap", "HoodRetailClothing"),
        ("HoodClothingUniformTankWorkpants", "HoodClothingTankWorkpants", "HoodRetailClothing"),
        ("HoodClothingUniformForestPolo", "HoodClothingForestPolo", "HoodRetailClothing"),
        ("HoodClothingOuterCharcoalPuffer", "HoodClothingCharcoalPuffer", "HoodRetailClothing"),
        ("HoodClothingOuterBrownWorkJacket", "HoodClothingBrownWorkJacket", "HoodRetailClothing"),
        ("HoodClothingHeadBlackKnitBeanie", "HoodClothingBlackKnitBeanie", "HoodRetailClothing"),
        ("HoodClothingEyesSmokeRectangular", "HoodClothingSmokeRectangular", "HoodRetailClothing"),
    };

    [Test]
    public async Task NeutralWardrobeSpawnsAndUsesDollarRetail()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var testMap = await pair.CreateTestMap();
        var entities = server.ResolveDependency<IEntityManager>();
        var prototypes = server.ResolveDependency<IPrototypeManager>();

        await server.WaitAssertion(() =>
        {
            foreach (var (entityId, listingId, categoryId) in Wardrobe)
            {
                var uid = entities.SpawnEntity(entityId, testMap.GridCoords);
                Assert.Multiple(() =>
                {
                    Assert.That(entities.HasComponent<ClothingComponent>(uid), Is.True, entityId);
                });

                var listing = prototypes.Index<ListingPrototype>(listingId);
                Assert.Multiple(() =>
                {
                    Assert.That(listing.ProductEntity?.Id, Is.EqualTo(entityId), listingId);
                    Assert.That(listing.Categories.Select(category => category.Id),
                        Is.EquivalentTo(new[] { categoryId }), listingId);
                    Assert.That(listing.Cost.Keys.Select(currency => currency.Id),
                        Is.EquivalentTo(new[] { "HoodDollar" }), listingId);
                    Assert.That(listing.Cost["HoodDollar"].Int(), Is.GreaterThan(0), listingId);
                });
            }
        });

        await pair.CleanReturnAsync();
    }
}
