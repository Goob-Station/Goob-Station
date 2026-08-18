// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.Lathe;
using Content.Shared.Lathe.Prototypes;
using Content.Shared.Research.Prototypes;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests._Hood.Weapons;

[TestFixture]
public sealed class UndergroundPrinterTest
{
    [Test]
    public async Task PrinterHasOnlyItsRestrictedStaticPack()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var prototypes = server.ResolveDependency<IPrototypeManager>();
        var components = server.ResolveDependency<IComponentFactory>();

        await server.WaitAssertion(() =>
        {
            var printer = prototypes.Index<EntityPrototype>("HoodUndergroundPrinter");
            var latheName = components.GetComponentName<LatheComponent>();
            Assert.That(printer.TryGetComponent(latheName, out LatheComponent? lathe), Is.True);

            Assert.Multiple(() =>
            {
                Assert.That(lathe!.StaticPacks.Select(id => id.Id),
                    Is.EquivalentTo(new[] { "HoodUndergroundStatic" }));
                Assert.That(lathe.DynamicPacks, Is.Empty);
            });

            var pack = prototypes.Index<LatheRecipePackPrototype>("HoodUndergroundStatic");
            Assert.That(pack.Recipes.Select(id => id.Id),
                Is.EquivalentTo(new[] { "HoodGunSwitchRecipe" }));

            var recipe = prototypes.Index<LatheRecipePrototype>("HoodGunSwitchRecipe");
            Assert.Multiple(() =>
            {
                Assert.That(recipe.Result?.Id, Is.EqualTo("HoodGunSwitch"));
                Assert.That(recipe.Materials.Keys.Select(id => id.Id),
                    Is.EquivalentTo(new[] { "Steel", "Plastic" }));
            });

            var autolathe = prototypes.Index<EntityPrototype>("Autolathe");
            Assert.That(autolathe.TryGetComponent(latheName, out LatheComponent? normalLathe), Is.True);
            Assert.That(normalLathe!.StaticPacks.Select(id => id.Id),
                Does.Not.Contain("HoodUndergroundStatic"));
        });

        await pair.CleanReturnAsync();
    }
}
