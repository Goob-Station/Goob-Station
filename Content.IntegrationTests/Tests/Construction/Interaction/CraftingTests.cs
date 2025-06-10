// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Alice "Arimah" Heurlin <30327355+arimah@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Flareguy <78941145+Flareguy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 HS <81934438+HolySSSS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Hanz <41141796+Hanzdegloker@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 IProduceWidgets <107586145+IProduceWidgets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mr. 27 <45323883+Dutch-VanDerLinde@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Rouge2t7 <81053047+Sarahon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 Truoizys <153248924+Truoizys@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 TsjipTsjip <19798667+TsjipTsjip@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ubaser <134914314+UbaserB@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Vasilis <vasilis@pikachu.systems>
// SPDX-FileCopyrightText: 2024 beck-thompson <107373427+beck-thompson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 osjarw <62134478+osjarw@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 plykiya <plykiya@protonmail.com>
// SPDX-FileCopyrightText: 2024 Арт <123451459+JustArt1m@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.IntegrationTests.Tests.Interaction;
using Content.Shared.DoAfter;
using Content.Shared.Stacks;
using Robust.Shared.Containers;

namespace Content.IntegrationTests.Tests.Construction.Interaction;

public sealed class CraftingTests : InteractionTest
{
    public const string ShardGlass = "ShardGlass";
    public const string Spear = "Spear";

    /// <summary>
    /// Craft a simple instant recipe
    /// </summary>
    [Test]
    public async Task CraftRods()
    {
        await PlaceInHands(Steel);
        await CraftItem(Rod);
        await FindEntity((Rod, 2));
    }

    /// <summary>
    /// Craft a simple recipe with a DoAfter
    /// </summary>
    [Test]
    public async Task CraftGrenade()
    {
        await PlaceInHands(Steel, 5);
        await CraftItem("ModularGrenadeRecipe");
        await FindEntity("ModularGrenade");
    }

    /// <summary>
    /// Craft a complex recipe (more than one ingredient).
    /// </summary>
    [Test]
    public async Task CraftSpear()
    {
        // Spawn a full tack of rods in the user's hands.
        await PlaceInHands(Rod, 10);
        await SpawnEntity((Cable, 10), SEntMan.GetCoordinates(PlayerCoords));

        // Attempt (and fail) to craft without glass.
        await CraftItem(Spear, shouldSucceed: false);
        await FindEntity(Spear, shouldSucceed: false);

        // Spawn three shards of glass and finish crafting (only one is needed).
        await SpawnTarget(ShardGlass);
        await SpawnTarget(ShardGlass);
        await SpawnTarget(ShardGlass);
        await CraftItem(Spear);
        await FindEntity(Spear);

        // Reset target because entitylookup will dump this.
        Target = null;

        // Player's hands should be full of the remaining rods, except those dropped during the failed crafting attempt.
        // Spear and left over stacks should be on the floor.
        await AssertEntityLookup((Rod, 2), (Cable, 7), (ShardGlass, 2), (Spear, 1));
    }

    /// <summary>
    /// Cancel crafting a complex recipe.
    /// </summary>
    [Test]
    public async Task CancelCraft()
    {
        var serverTargetCoords = SEntMan.GetCoordinates(TargetCoords);
        var rods = await SpawnEntity((Rod, 10), serverTargetCoords);
        var wires = await SpawnEntity((Cable, 10), serverTargetCoords);
        var shard = await SpawnEntity(ShardGlass, serverTargetCoords);

        var rodStack = SEntMan.GetComponent<StackComponent>(rods);
        var wireStack = SEntMan.GetComponent<StackComponent>(wires);

        await RunTicks(5);
        var sys = SEntMan.System<SharedContainerSystem>();
        Assert.Multiple(() =>
        {
            Assert.That(sys.IsEntityInContainer(rods), Is.False);
            Assert.That(sys.IsEntityInContainer(wires), Is.False);
            Assert.That(sys.IsEntityInContainer(shard), Is.False);
        });

#pragma warning disable CS4014 // Legacy construction code uses DoAfterAwait. If we await it we will be waiting forever.
        await Server.WaitPost(() => SConstruction.TryStartItemConstruction(Spear, SEntMan.GetEntity(Player)));
#pragma warning restore CS4014
        await RunTicks(1);

        // DoAfter is in progress. Entity not spawned, stacks have been split and someingredients are in a container.
        Assert.That(ActiveDoAfters.Count(), Is.EqualTo(1));
        Assert.That(sys.IsEntityInContainer(shard), Is.True);
        Assert.That(sys.IsEntityInContainer(rods), Is.False);
        Assert.That(sys.IsEntityInContainer(wires), Is.False);
        Assert.That(rodStack, Has.Count.EqualTo(8));
        Assert.That(wireStack, Has.Count.EqualTo(7));

        await FindEntity(Spear, shouldSucceed: false);

        // Cancel the DoAfter. Should drop ingredients to the floor.
        await CancelDoAfters();
        Assert.That(sys.IsEntityInContainer(rods), Is.False);
        Assert.That(sys.IsEntityInContainer(wires), Is.False);
        Assert.That(sys.IsEntityInContainer(shard), Is.False);
        await FindEntity(Spear, shouldSucceed: false);
        await AssertEntityLookup((Rod, 10), (Cable, 10), (ShardGlass, 1));

        // Re-attempt the do-after
#pragma warning disable CS4014 // Legacy construction code uses DoAfterAwait. See above.
        await Server.WaitPost(() => SConstruction.TryStartItemConstruction(Spear, SEntMan.GetEntity(Player)));
#pragma warning restore CS4014
        await RunTicks(1);

        // DoAfter is in progress. Entity not spawned, ingredients are in a container.
        Assert.That(ActiveDoAfters.Count(), Is.EqualTo(1));
        Assert.That(sys.IsEntityInContainer(shard), Is.True);
        await FindEntity(Spear, shouldSucceed: false);

        // Finish the DoAfter
        await AwaitDoAfters();

        // Spear has been crafted. Rods and wires are no longer contained. Glass has been consumed.
        await FindEntity(Spear);
        Assert.That(sys.IsEntityInContainer(rods), Is.False);
        Assert.That(sys.IsEntityInContainer(wires), Is.False);
        Assert.That(SEntMan.Deleted(shard));
    }
}