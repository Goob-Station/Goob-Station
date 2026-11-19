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
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Stacks;
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
            Assert.That(SEntMan.System<ItemToggleSystem>().IsActivated(barrel), "The lid should be off");
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

    [Test]
    public async Task DrinkMatchesTheFoodInTheCrop()
    {
        await SpawnTarget("FermentingBarrel");
        var barrel = STarget!.Value;
        await Interact();
        await InteractUsing("FoodGrape");

        FixedPoint2 food = 0, other = 0;
        await Server.WaitAssertion(() =>
        {
            Assert.That(SEntMan.System<SharedSolutionContainerSystem>().TryGetSolution(Crops(barrel)[0], "food", out _, out var crop));
            food = crop!.GetTotalPrototypeQuantity("Nutriment") + crop.GetTotalPrototypeQuantity("Vitamin");
            other = crop.Volume - food;
        });

        await Interact();
        await RunSeconds(5);

        await Server.WaitAssertion(() =>
        {
            Assert.That(Contents(barrel).GetTotalPrototypeQuantity("Wine"), Is.EqualTo(food));
            Assert.That(Contents(barrel).Volume - Contents(barrel).GetTotalPrototypeQuantity("Wine"), Is.EqualTo(other), "Everything else goes in as it is");
        });
    }

    [Test]
    public async Task OnlyGrainsFermentTheirSugar()
    {
        await SpawnTarget("FermentingBarrel");
        var barrel = STarget!.Value;
        await Interact();
        await InteractUsing("FoodLaughinPeaPod");
        await Interact();
        await RunSeconds(5);

        await Server.WaitAssertion(() => Assert.That(Contents(barrel).GetTotalPrototypeQuantity("Sugar").Float(), Is.GreaterThan(0), "Peas keep their sugar"));
    }

    [Test]
    public async Task DriedTobaccoGoesInALeafAtATime()
    {
        await SpawnTarget("FermentingBarrel");
        var barrel = STarget!.Value;
        await Interact();
        var leaves = await PlaceInHands("LeavesTobaccoDried", 3);
        await Interact();

        await Server.WaitAssertion(() =>
        {
            Assert.That(Crops(barrel), Has.Count.EqualTo(1));
            Assert.That(SEntMan.GetComponent<StackComponent>(SEntMan.GetEntity(leaves)).Count, Is.EqualTo(2));
        });

        await Activate(); // shut the lid; a click would put in another leaf
        await RunSeconds(5);
        await Server.WaitAssertion(() =>
        {
            Assert.That(Crops(barrel), Is.Empty);
            Assert.That(Contents(barrel).GetTotalPrototypeQuantity("CremeDeMenthe").Float(), Is.GreaterThan(0));
            Assert.That(Contents(barrel).GetTotalPrototypeQuantity("Nicotine"),
                Is.EqualTo(Contents(barrel).GetTotalPrototypeQuantity("CremeDeMenthe")), "Half the nicotine stays");
        });
    }

    [Test]
    [TestCase("Sugarcane", "Rum", "Sugar", 0.29f)]
    [TestCase("FoodCorn", "Whiskey", "Nutriment", 1f)]
    [TestCase("FoodAloe", "Tequila", "Aloe", 1f)]
    public async Task CropsMakeTgsShareOfDrink(string crop, string drink, string source, float share)
    {
        await SpawnTarget("FermentingBarrel");
        var barrel = STarget!.Value;
        await Interact();
        await InteractUsing(crop);

        FixedPoint2 expected = 0, total = 0;
        await Server.WaitAssertion(() =>
        {
            Assert.That(SEntMan.System<SharedSolutionContainerSystem>().TryGetSolution(Crops(barrel)[0], "food", out _, out var contents));
            expected = contents!.GetTotalPrototypeQuantity(source) * share;
            if (crop == "FoodAloe")
                expected += contents.GetTotalPrototypeQuantity("SilverSulfadiazine");
            total = contents.Volume;
        });

        await Interact();
        await RunSeconds(5);
        await Server.WaitAssertion(() =>
        {
            Assert.That(Contents(barrel).GetTotalPrototypeQuantity(drink), Is.EqualTo(expected));
            Assert.That(Contents(barrel).Volume, Is.EqualTo(total), "Nothing is lost");
        });
    }

    [Test]
    public async Task BreakingItLosesTheCrops()
    {
        await SpawnTarget("FermentingBarrel");
        var barrel = STarget!.Value;
        await Interact();
        await InteractUsing("FoodGrape");

        await Server.WaitPost(() =>
        {
            var blunt = new DamageSpecifier(ProtoMan.Index<DamageTypePrototype>("Blunt"), 400);
            SEntMan.System<DamageableSystem>().TryChangeDamage(barrel, blunt, ignoreResistances: true);
        });
        await RunTicks(5);

        await Server.WaitAssertion(() =>
        {
            Assert.That(SEntMan.Deleted(barrel));
            var left = SEntMan.AllEntities<MetaDataComponent>()
                .Count(e => e.Comp.EntityPrototype?.ID is "FoodGrape" or "MaterialWoodPlank1" or "MaterialWoodPlank");
            Assert.That(left, Is.Zero, "Nothing drops");
        });
    }

    [Test]
    public async Task OverflowIsLostAndFermentingStops()
    {
        await SpawnTarget("FermentingBarrel");
        var barrel = STarget!.Value;
        await Interact();
        await InteractUsing("FoodGrape");
        await InteractUsing("FoodGrape");

        await Server.WaitAssertion(() =>
        {
            var solutions = SEntMan.System<SharedSolutionContainerSystem>();
            Assert.That(solutions.TryGetSolution(barrel, "barrel", out var solution, out var contents));
            solutions.TryAddReagent(solution!.Value, "Water", contents!.AvailableVolume - 1, out _);
        });

        await Interact();
        await RunSeconds(5);
        await Server.WaitAssertion(() =>
        {
            Assert.That(Contents(barrel).AvailableVolume, Is.EqualTo(FixedPoint2.Zero));
            Assert.That(Crops(barrel), Has.Count.EqualTo(1), "Fermenting stops once it's full");
            Assert.That(SEntMan.HasComponent<ActiveFermentingBarrelComponent>(barrel), Is.False);
        });
    }

    [Test]
    public async Task OpeningStopsFermenting()
    {
        await SpawnTarget("FermentingBarrel");
        var barrel = STarget!.Value;
        await Interact();
        await InteractUsing("FoodGrape");
        await Interact();
        await Server.WaitAssertion(() => Assert.That(SEntMan.HasComponent<ActiveFermentingBarrelComponent>(barrel)));

        await Interact();
        await RunSeconds(5);
        await Server.WaitAssertion(() =>
        {
            Assert.That(SEntMan.HasComponent<ActiveFermentingBarrelComponent>(barrel), Is.False);
            Assert.That(Crops(barrel), Has.Count.EqualTo(1));
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
