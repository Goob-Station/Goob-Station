// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.CCVar;
using Content.Shared.Salvage;
using Robust.Shared.Configuration;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed class SalvageTest
{
    /// <summary>
    /// Asserts that all salvage maps have been saved as grids and are loadable.
    /// </summary>
    [Test]
    public async Task AllSalvageMapsLoadableTest()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;

        var entManager = server.ResolveDependency<IEntityManager>();
        var mapLoader = entManager.System<MapLoaderSystem>();
        var prototypeManager = server.ResolveDependency<IPrototypeManager>();
        var cfg = server.ResolveDependency<IConfigurationManager>();
        var mapSystem = entManager.System<SharedMapSystem>();
        Assert.That(cfg.GetCVar(CCVars.GridFill), Is.False);

        await server.WaitPost(() =>
        {
            foreach (var salvage in prototypeManager.EnumeratePrototypes<SalvageMapPrototype>())
            {
                mapSystem.CreateMap(out var mapId);
                try
                {
                    Assert.That(mapLoader.TryLoadGrid(mapId, salvage.MapPath, out var grid));
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to load salvage map {salvage.ID}, was it saved as a map instead of a grid?", ex);
                }

                try
                {
                    mapSystem.DeleteMap(mapId);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to delete salvage map {salvage.ID}", ex);
                }
            }
        });
        await server.WaitRunTicks(1);

        await pair.CleanReturnAsync();
    }
}