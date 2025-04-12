// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Alice "Arimah" Heurlin <30327355+arimah@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Flareguy <78941145+Flareguy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 HS <81934438+HolySSSS@users.noreply.github.com>
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

using Content.Shared.Alert;
using Content.Shared.Buckle.Components;
using Robust.Shared.Maths;

namespace Content.IntegrationTests.Tests.Movement;

public sealed class BuckleMovementTest : MovementTest
{
    // Check that interacting with a chair straps you to it and prevents movement.
    [Test]
    public async Task ChairTest()
    {
        await SpawnTarget("Chair");

        var cAlert = Client.System<AlertsSystem>();
        var sAlert = Server.System<AlertsSystem>();
        var buckle = Comp<BuckleComponent>(Player);
        var strap = Comp<StrapComponent>(Target);

#pragma warning disable RA0002
        buckle.Delay = TimeSpan.Zero;
#pragma warning restore RA0002

        // Initially not buckled to the chair, and standing off to the side
        Assert.That(Delta(), Is.InRange(0.9f, 1.1f));
        Assert.That(buckle.Buckled, Is.False);
        Assert.That(buckle.BuckledTo, Is.Null);
        Assert.That(strap.BuckledEntities, Is.Empty);
        Assert.That(cAlert.IsShowingAlert(CPlayer, strap.BuckledAlertType), Is.False);
        Assert.That(sAlert.IsShowingAlert(SPlayer, strap.BuckledAlertType), Is.False);

        // Interact results in being buckled to the chair
        await Interact();
        Assert.That(Delta(), Is.InRange(-0.01f, 0.01f));
        Assert.That(buckle.Buckled, Is.True);
        Assert.That(buckle.BuckledTo, Is.EqualTo(STarget));
        Assert.That(strap.BuckledEntities, Is.EquivalentTo(new[] { SPlayer }));
        Assert.That(cAlert.IsShowingAlert(CPlayer, strap.BuckledAlertType), Is.True);
        Assert.That(sAlert.IsShowingAlert(SPlayer, strap.BuckledAlertType), Is.True);

        // Attempting to walk away does nothing
        await Move(DirectionFlag.East, 1);
        Assert.That(Delta(), Is.InRange(-0.01f, 0.01f));
        Assert.That(buckle.Buckled, Is.True);
        Assert.That(buckle.BuckledTo, Is.EqualTo(STarget));
        Assert.That(strap.BuckledEntities, Is.EquivalentTo(new[] { SPlayer }));
        Assert.That(cAlert.IsShowingAlert(CPlayer, strap.BuckledAlertType), Is.True);
        Assert.That(sAlert.IsShowingAlert(SPlayer, strap.BuckledAlertType), Is.True);

        // Interacting again will unbuckle the player
        await Interact();
        Assert.That(Delta(), Is.InRange(-0.5f, 0.5f));
        Assert.That(buckle.Buckled, Is.False);
        Assert.That(buckle.BuckledTo, Is.Null);
        Assert.That(strap.BuckledEntities, Is.Empty);
        Assert.That(cAlert.IsShowingAlert(CPlayer, strap.BuckledAlertType), Is.False);
        Assert.That(sAlert.IsShowingAlert(SPlayer, strap.BuckledAlertType), Is.False);

        // And now they can move away
        await Move(DirectionFlag.SouthEast, 1);
        Assert.That(Delta(), Is.LessThan(-1));
    }
}