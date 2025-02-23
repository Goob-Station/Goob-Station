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
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { DummyTicker = false, Dirty = true, Fresh = true});
        var server = pair.Server;
        var entMan = server.EntMan;
        var protoMan = server.ProtoMan;
        var mapMan = server.MapMan;

        var ticker = server.System<GameTicker>();
        var lavaSystem = entMan.System<LavalandPlanetSystem>();
        var mapSystem = entMan.System<SharedMapSystem>();

        // Setup
        pair.Server.CfgMan.SetCVar(CCVars.LavalandEnabled, false);

        await server.WaitPost(() => ticker.RestartRound());
        await pair.RunTicksSync(25);
        Assert.That(ticker.RunLevel, Is.EqualTo(GameRunLevel.InRound));

        // Seed is always the same to reduce randomness
        const int seed = 1;

        var attempt = false;
        Entity<LavalandMapComponent>? lavaland = null;

        // Generate all lavalands
        await server.WaitPost(() => attempt = lavaSystem.SetupLavaland(out lavaland, seed));
        await pair.RunTicksSync(30);

        // Get all possible types of Lavaland and test them.
        Assert.That(lavaland, Is.Not.Null);

        var mapId = lavaland.Value.Comp.MapId;

        // Now check the basics
        Assert.That(attempt, Is.True);
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

        entMan.QueueDeleteEntity(lavaland);

        await pair.RunTicksSync(10);

        await pair.CleanReturnAsync();
    }
}
