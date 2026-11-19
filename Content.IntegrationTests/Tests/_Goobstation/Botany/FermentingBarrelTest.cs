// SPDX-License-Identifier: AGPL-3.0-or-later

#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using Content.Goobstation.Server.Botany;
using Content.IntegrationTests.Tests.Interaction;
using Content.Shared.Audio;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using NUnit.Framework;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;

namespace Content.IntegrationTests.Tests._Goobstation.Botany;

[TestFixture]
public sealed class FermentingBarrelTest : InteractionTest
{
    [Test]
    [TestCase("FoodGrape", "Wine")]
    [TestCase("WheatBushel", "Beer")]
    [TestCase("FoodApple", "Cider")]
    [TestCase("FoodLemon", "FruitWine")]
    public async Task CropFerments(string crop, string drink)
    {
        await SpawnTarget("FermentingBarrel");
        var barrel = STarget!.Value;

        await Interact();
        await InteractUsing(crop);
        await Server.WaitAssertion(() =>
        {
            Assert.That(SEntMan.GetComponent<FermentingBarrelComponent>(barrel).Open);
            Assert.That(Crops(barrel), Has.Count.EqualTo(1));
        });

        await Interact();
        await Server.WaitAssertion(() => Assert.That(SEntMan.GetComponent<AmbientSoundComponent>(barrel).Enabled, "Should bubble while fermenting"));

        await RunSeconds(5);
        await Server.WaitAssertion(() =>
        {
            Assert.That(Crops(barrel), Is.Empty);
            Assert.That(SEntMan.GetComponent<AmbientSoundComponent>(barrel).Enabled, Is.False, "Should stop bubbling once done");
            Assert.That(SEntMan.System<SharedSolutionContainerSystem>().TryGetSolution(barrel, "barrel", out _, out var contents));
            Assert.That(contents!.GetTotalPrototypeQuantity(drink).Float(), Is.GreaterThan(0));
        });
    }

    [Test]
    public async Task LidDecidesWhichWayLiquidGoes()
    {
        await SpawnTarget("FermentingBarrel");
        var barrel = STarget!.Value;

        await Server.WaitAssertion(() =>
        {
            Assert.That(SEntMan.HasComponent<DrainableSolutionComponent>(barrel), "The tap works with the lid on");
            Assert.That(SEntMan.HasComponent<RefillableSolutionComponent>(barrel), Is.False);
            Assert.That(SEntMan.HasComponent<ExaminableSolutionComponent>(barrel), Is.False);
        });

        await Interact();
        await Server.WaitAssertion(() =>
        {
            Assert.That(SEntMan.HasComponent<DrainableSolutionComponent>(barrel), Is.False);
            Assert.That(SEntMan.HasComponent<RefillableSolutionComponent>(barrel), "Liquid pours in with the lid off");
            Assert.That(SEntMan.HasComponent<ExaminableSolutionComponent>(barrel));
        });
    }

    [Test]
    public async Task ClosedBarrelRefusesCrops()
    {
        await SpawnTarget("FermentingBarrel");
        await InteractUsing("FoodPotato");

        await Server.WaitAssertion(() => Assert.That(Crops(STarget!.Value), Is.Empty));
    }

    [Test]
    public async Task SomeCropsWontFerment()
    {
        await SpawnTarget("FermentingBarrel");
        await Interact();
        await InteractUsing("MeatwheatBushel");

        await Server.WaitAssertion(() => Assert.That(Crops(STarget!.Value), Is.Empty));
    }

    private IReadOnlyList<EntityUid> Crops(EntityUid barrel)
    {
        return SEntMan.System<SharedContainerSystem>().GetContainer(barrel, "produce").ContainedEntities;
    }
}
