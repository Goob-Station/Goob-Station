// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.IntegrationTests.Tests.Interaction;
using Content.Shared.Placeable;

namespace Content.IntegrationTests.Tests.Construction.Interaction;

public sealed class PlaceableDeconstruction : InteractionTest
{
    /// <summary>
    /// Checks that you can deconstruct placeable surfaces (i.e., placing a wrench on a table does not take priority).
    /// </summary>
    [Test]
    public async Task DeconstructTable()
    {
        await StartDeconstruction("Table");
        Assert.That(Comp<PlaceableSurfaceComponent>().IsPlaceable);
        await InteractUsing(Wrench);
        AssertPrototype("TableFrame");
        await InteractUsing(Wrench);
        AssertDeleted();
        await AssertEntityLookup((Steel, 1), (Rod, 2));
    }
}
