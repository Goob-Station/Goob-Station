// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.IntegrationTests.Tests.Interaction;
using Content.Shared.DoAfter;
using Content.Shared.Wires;

namespace Content.IntegrationTests.Tests.Construction.Interaction;

public sealed class PanelScrewing : InteractionTest
{
    // Test wires panel on both airlocks & tcomms servers. These both use the same component, but comms may have
    // conflicting interactions due to encryption key removal interactions.
    [Test]
    [TestCase("Airlock")]
    [TestCase("TelecomServerFilled")]
    public async Task WiresPanelScrewing(string prototype)
    {
        await SpawnTarget(prototype);
        var comp = Comp<WiresPanelComponent>();

        // Open & close panel
        Assert.That(comp.Open, Is.False);
        await InteractUsing(Screw);
        Assert.That(comp.Open, Is.True);
        await InteractUsing(Screw);
        Assert.That(comp.Open, Is.False);

        // Interrupted DoAfters
        await InteractUsing(Screw, awaitDoAfters: false);
        await CancelDoAfters();
        Assert.That(comp.Open, Is.False);
        await InteractUsing(Screw);
        Assert.That(comp.Open, Is.True);
        await InteractUsing(Screw, awaitDoAfters: false);
        await CancelDoAfters();
        Assert.That(comp.Open, Is.True);
        await InteractUsing(Screw);
        Assert.That(comp.Open, Is.False);
    }
}
