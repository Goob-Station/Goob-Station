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

#nullable enable
using Content.Shared.Alert;
using Content.Shared.Input;
using Content.Shared.Movement.Pulling.Components;
using Robust.Shared.Maths;

namespace Content.IntegrationTests.Tests.Movement;

public sealed class PullingTest : MovementTest
{
    protected override int Tiles => 4;

    [Test]
    public async Task PullTest()
    {
        var cAlert = Client.System<AlertsSystem>();
        var sAlert = Server.System<AlertsSystem>();
        await SpawnTarget("MobHuman");

        var puller = Comp<PullerComponent>(Player);
        var pullable = Comp<PullableComponent>(Target);

        // Player is initially to the left of the target and not pulling anything
        Assert.That(Delta(), Is.InRange(0.9f, 1.1f));
        Assert.That(puller.Pulling, Is.Null);
        Assert.That(pullable.Puller, Is.Null);
        Assert.That(pullable.BeingPulled, Is.False);
        Assert.That(cAlert.IsShowingAlert(CPlayer, puller.PullingAlert), Is.False);
        Assert.That(sAlert.IsShowingAlert(SPlayer, puller.PullingAlert), Is.False);

        // Start pulling
        await PressKey(ContentKeyFunctions.TryPullObject);
        await RunTicks(5);
        Assert.That(puller.Pulling, Is.EqualTo(STarget));
        Assert.That(pullable.Puller, Is.EqualTo(SPlayer));
        Assert.That(pullable.BeingPulled, Is.True);
        Assert.That(cAlert.IsShowingAlert(CPlayer, puller.PullingAlert), Is.True);
        Assert.That(sAlert.IsShowingAlert(SPlayer, puller.PullingAlert), Is.True);

        // Move to the left and check that the target moves with the player and is still being pulled.
        await Move(DirectionFlag.West, 1);
        Assert.That(Delta(), Is.InRange(0.9f, 1.3f));
        Assert.That(puller.Pulling, Is.EqualTo(STarget));
        Assert.That(pullable.Puller, Is.EqualTo(SPlayer));
        Assert.That(pullable.BeingPulled, Is.True);
        Assert.That(cAlert.IsShowingAlert(CPlayer, puller.PullingAlert), Is.True);
        Assert.That(sAlert.IsShowingAlert(SPlayer, puller.PullingAlert), Is.True);

        // Move in the other direction
        await Move(DirectionFlag.East, 2);
        Assert.That(Delta(), Is.InRange(-1.3f, -0.9f));
        Assert.That(puller.Pulling, Is.EqualTo(STarget));
        Assert.That(pullable.Puller, Is.EqualTo(SPlayer));
        Assert.That(pullable.BeingPulled, Is.True);
        Assert.That(cAlert.IsShowingAlert(CPlayer, puller.PullingAlert), Is.True);
        Assert.That(sAlert.IsShowingAlert(SPlayer, puller.PullingAlert), Is.True);

        // Stop pulling
        await PressKey(ContentKeyFunctions.ReleasePulledObject);
        await RunTicks(5);
        Assert.That(Delta(), Is.InRange(-1.3f, -0.9f));
        Assert.That(puller.Pulling, Is.Null);
        Assert.That(pullable.Puller, Is.Null);
        Assert.That(pullable.BeingPulled, Is.False);
        Assert.That(cAlert.IsShowingAlert(CPlayer, puller.PullingAlert), Is.False);
        Assert.That(sAlert.IsShowingAlert(SPlayer, puller.PullingAlert), Is.False);

        // Move back to the left and ensure the target is no longer following us.
        await Move(DirectionFlag.West, 2);
        Assert.That(Delta(), Is.GreaterThan(2f));
    }
}
