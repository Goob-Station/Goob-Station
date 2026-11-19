// SPDX-License-Identifier: AGPL-3.0-or-later

#nullable enable
using System.Threading.Tasks;
using Content.IntegrationTests.Tests.Interaction;
using Content.Goobstation.Server.Botany;
using Content.Shared.Chemistry.EntitySystems;
using NUnit.Framework;
using Robust.Shared.Containers;

namespace Content.IntegrationTests.Tests._Goobstation.Botany;

[TestFixture]
public sealed class FermentingBarrelTest : InteractionTest
{
    [Test]
    [TestCase("FoodGrape", "Wine")]
    [TestCase("WheatBushel", "Beer")]
    [TestCase("FoodApple", "FruitWine")]
    public async Task CropFerments(string crop, string drink)
    {
        await SpawnTarget("FermentingBarrel");
        var barrel = STarget!.Value;

        await Interact();
        await InteractUsing(crop);
        await Server.WaitAssertion(() =>
        {
            Assert.That(SEntMan.GetComponent<FermentingBarrelComponent>(barrel).Open);
            Assert.That(SEntMan.System<SharedContainerSystem>().GetContainer(barrel, "produce").ContainedEntities, Has.Count.EqualTo(1));
        });

        await Interact();
        await RunSeconds(5);

        await Server.WaitAssertion(() =>
        {
            Assert.That(SEntMan.GetComponent<FermentingBarrelComponent>(barrel).Open, Is.False);
            Assert.That(SEntMan.System<SharedContainerSystem>().GetContainer(barrel, "produce").ContainedEntities, Is.Empty);
            Assert.That(SEntMan.System<SharedSolutionContainerSystem>().TryGetSolution(barrel, "barrel", out _, out var contents));
            Assert.That(contents!.GetTotalPrototypeQuantity(drink).Float(), Is.GreaterThan(0));
        });
    }

    [Test]
    public async Task ClosedBarrelRefusesCrops()
    {
        await SpawnTarget("FermentingBarrel");
        await InteractUsing("FoodPotato");

        await Server.WaitAssertion(() =>
        {
            Assert.That(SEntMan.System<SharedContainerSystem>().GetContainer(STarget!.Value, "produce").ContainedEntities, Is.Empty);
        });
    }
}
