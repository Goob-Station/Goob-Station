// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.IntegrationTests.Tests.Interaction;
using Content.Shared.Tools.Components;

namespace Content.IntegrationTests.Tests.Weldable;

/// <summary>
///  Simple test to check that using a welder on a locker will weld it shut.
/// </summary>
public sealed class WeldableTests : InteractionTest
{
    public const string Locker = "LockerFreezer";

    [Test]
    public async Task WeldLocker()
    {
        await SpawnTarget(Locker);
        var comp = Comp<WeldableComponent>();

        Assert.That(comp.IsWelded, Is.False);

        await InteractUsing(Weld);
        Assert.That(comp.IsWelded, Is.True);
        AssertPrototype(Locker); // Prototype did not change.
    }
}