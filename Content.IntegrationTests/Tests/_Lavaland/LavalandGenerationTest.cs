// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aineias1 <dmitri.s.kiselev@gmail.com>
// SPDX-FileCopyrightText: 2025 FaDeOkno <143940725+FaDeOkno@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 McBosserson <148172569+McBosserson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Milon <plmilonpl@gmail.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Unlumination <144041835+Unlumy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server._Lavaland.Procedural.Components;
using Content.Server._Lavaland.Procedural.Systems;
using Content.Server.GameTicking;
using Content.Shared._Lavaland.Procedural.Prototypes;
using Content.Shared.CCVar;
using Content.Shared.Parallax.Biomes;
using Robust.Shared.GameObjects;

namespace Content.IntegrationTests.Tests._Lavaland;

[TestFixture]
[TestOf(typeof(LavalandPlanetSystem))]
public sealed class LavalandGenerationTest
{
    [Test]
    public async Task LavalandPlanetGenerationTest()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
            { DummyTicker = false, Dirty = true, Fresh = true });
        var server = pair.Server;
        var entMan = server.EntMan;
        var protoMan = server.ProtoMan;
        var mapMan = server.MapMan;

        var ticker = server.System<GameTicker>();
        var lavaSystem = entMan.System<LavalandPlanetSystem>();
        var mapSystem = entMan.System<SharedMapSystem>();

        // Setup
        pair.Server.CfgMan.SetCVar(CCVars.LavalandEnabled, true);
        pair.Server.CfgMan.SetCVar(CCVars.GameDummyTicker, false);
        var gameMap = pair.Server.CfgMan.GetCVar(CCVars.GameMap);
        pair.Server.CfgMan.SetCVar(CCVars.GameMap, "Saltern");
        var gameMode = pair.Server.CfgMan.GetCVar(CCVars.GameLobbyDefaultPreset);
        pair.Server.CfgMan.SetCVar(CCVars.GameLobbyDefaultPreset, "secret");

        await server.WaitPost(() => ticker.RestartRound());
        await pair.RunTicksSync(25);
        Assert.That(ticker.RunLevel, Is.EqualTo(GameRunLevel.InRound));

        // Get all possible types of Lavaland and test them.
        var planets = protoMan.EnumeratePrototypes<LavalandMapPrototype>().ToList();
        foreach (var planet in planets)
        {
            const int seed = 1;

            var attempt = false;
            Entity<LavalandMapComponent>? lavaland = null;

            // Seed is always the same to reduce randomness
            await server.WaitPost(() => lavaSystem.EnsurePreloaderMap());
            await server.WaitPost(() => attempt = lavaSystem.SetupLavalandPlanet(out lavaland, planet, seed));
            await pair.RunTicksSync(30);

            Assert.That(attempt, Is.True);
            Assert.That(lavaland, Is.Not.Null);

            var mapId = entMan.GetComponent<TransformComponent>(lavaland.Value).MapID;

            // Now check the basics
            Assert.That(mapMan.MapExists(mapId));
            Assert.That(entMan.EntityExists(lavaland.Value.Owner));
            Assert.That(entMan.EntityExists(lavaland.Value.Comp.Outpost));
            Assert.That(mapMan.GetAllGrids(mapId).ToList(), Is.Not.Empty);
            Assert.That(mapSystem.IsInitialized(mapId));
            Assert.That(mapSystem.IsPaused(mapId), Is.False);

            // Test that the biome setup is right
            var biome = entMan.GetComponent<BiomeComponent>(lavaland.Value);
            Assert.That(biome.Enabled, Is.True);
            Assert.That(biome.Seed, Is.EqualTo(seed));
            Assert.That(biome.Template, Is.Not.Null);
            Assert.That(biome.Layers, Is.Not.Empty);
        }

        await pair.RunTicksSync(10);

        var lavalands = lavaSystem.GetLavalands();
        Assert.That(planets, Has.Count.EqualTo(lavalands.Count));

        // Cleanup
        foreach (var lavaland in lavalands)
        {
            entMan.QueueDeleteEntity(lavaland);
        }

        await pair.RunTicksSync(10);

        pair.Server.CfgMan.SetCVar(CCVars.GameMap, gameMap);
        pair.Server.CfgMan.SetCVar(CCVars.GameLobbyDefaultPreset, gameMode);
        pair.ClearModifiedCvars();
        await pair.CleanReturnAsync();
    }
}