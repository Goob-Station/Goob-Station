// SPDX-License-Identifier: AGPL-3.0-or-later

#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Content.Goobstation.Server.Botany;
using Content.IntegrationTests.Tests.Interaction;
using Content.Shared.Audio;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Storage.EntitySystems;
using Content.Shared.Verbs;
using NUnit.Framework;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;
using Robust.Shared.Localization;

namespace Content.IntegrationTests.Tests._Goobstation.Botany;

[TestFixture]
public sealed class FermentingBarrelTest : InteractionTest
{
    [Test]
    [TestCase("FoodGrape", "Wine")]
    [TestCase("WheatBushel", "Beer")]
    [TestCase("FoodApple", "Cider")]
    [TestCase("FoodLemon", "FruitWine")]
    [TestCase("FoodCherry", "Maraschino")]
    [TestCase("FoodOrange", "TripleSec")]
    [TestCase("FoodLily", "Vermouth")]
    [TestCase("FoodAloe", "Tequila")]
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
            Assert.That(SEntMan.HasComponent<ActiveFermentingBarrelComponent>(barrel), Is.False);
            Assert.That(Contents(barrel).GetTotalPrototypeQuantity(drink).Float(), Is.GreaterThan(0));
        });
    }

    [Test]
    public async Task CornTurnsWhollyToWhiskey()
    {
        await SpawnTarget("FermentingBarrel");
        var barrel = STarget!.Value;

        await Interact();
        await InteractUsing("FoodCorn");
        await Interact();
        await RunSeconds(5);

        await Server.WaitAssertion(() =>
        {
            Assert.That(Contents(barrel).GetTotalPrototypeQuantity("Whiskey").Float(), Is.GreaterThan(0));
            Assert.That(Contents(barrel).GetTotalPrototypeQuantity("Cornmeal").Float(), Is.EqualTo(0));
            Assert.That(Contents(barrel).GetTotalPrototypeQuantity("Nutriment").Float(), Is.EqualTo(0));
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
    [TestCase("MeatwheatBushel")]
    [TestCase("LeavesTea")]
    [TestCase("Log")]
    [TestCase("CottonBol")]
    public async Task RefusesNonFermentables(string item)
    {
        await SpawnTarget("FermentingBarrel");
        await Interact();
        await InteractUsing(item);

        await Server.WaitAssertion(() => Assert.That(Crops(STarget!.Value), Is.Empty));
    }

    [Test]
    public async Task DeathNettleGoesIn()
    {
        await SpawnTarget("FermentingBarrel");
        await Interact();
        await InteractUsing("DeathNettle");

        await Server.WaitAssertion(() => Assert.That(Crops(STarget!.Value), Has.Count.EqualTo(1)));
    }

    [Test]
    [TestCase("PlantBag", 2)]
    [TestCase("ClothingBackpack", 0)]
    public async Task BagsTipCropsIn(string bagProto, int expected)
    {
        await SpawnTarget("FermentingBarrel");
        await Interact();
        var bag = await PlaceInHands(bagProto);
        var coords = SEntMan.GetCoordinates(PlayerCoords);
        var grapes = new[] { await SpawnEntity("FoodGrape", coords), await SpawnEntity("FoodGrape", coords) };

        await Server.WaitAssertion(() =>
        {
            foreach (var grape in grapes)
            {
                Assert.That(SEntMan.System<SharedStorageSystem>().Insert(SEntMan.GetEntity(bag), grape, out _, playSound: false));
            }
        });

        await Interact();
        await Server.WaitAssertion(() => Assert.That(Crops(STarget!.Value), Has.Count.EqualTo(expected)));
    }

    [Test]
    public async Task FullBarrelRefusesCrops()
    {
        await SpawnTarget("FermentingBarrel");
        var barrel = STarget!.Value;
        await Interact();

        await Server.WaitAssertion(() =>
        {
            var solutions = SEntMan.System<SharedSolutionContainerSystem>();
            Assert.That(solutions.TryGetSolution(barrel, "barrel", out var solution, out var contents));
            solutions.TryAddReagent(solution!.Value, "Water", contents!.AvailableVolume - 1, out _);
        });

        // Room is checked against what's already waiting, so the first crop still goes in and the second doesn't.
        await InteractUsing("FoodGrape");
        await InteractUsing("FoodGrape");
        await Server.WaitAssertion(() => Assert.That(Crops(barrel), Has.Count.EqualTo(1)));
    }

    [Test]
    public async Task CropsTipBackOut()
    {
        await SpawnTarget("FermentingBarrel");
        var barrel = STarget!.Value;
        await Interact();
        await InteractUsing("FoodGrape");

        await Server.WaitAssertion(() =>
        {
            var verbs = SEntMan.System<SharedVerbSystem>().GetLocalVerbs(barrel, SPlayer, typeof(AlternativeVerb));
            var tip = verbs.Single(v => v.Text == Loc.GetString("fermenting-barrel-empty-verb"));
            tip.Act!.Invoke();
            Assert.That(Crops(barrel), Is.Empty);
        });
    }

    private Solution Contents(EntityUid barrel)
    {
        Assert.That(SEntMan.System<SharedSolutionContainerSystem>().TryGetSolution(barrel, "barrel", out _, out var contents));
        return contents!;
    }

    private IReadOnlyList<EntityUid> Crops(EntityUid barrel)
    {
        return SEntMan.System<SharedContainerSystem>().GetContainer(barrel, "produce").ContainedEntities;
    }
}
