using Content.Goobstation.Common.Traitor;
using Content.Goobstation.Server.Traitor.PenSpin;
using Content.Goobstation.Shared.Traitor.PenSpin;
using Content.Server.GameTicking;
using Content.Server.Traitor.Uplink;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.PDA;
using Content.Shared.Store.Components;
using Content.Shared.Tag;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;

namespace Content.IntegrationTests.Tests.Goobstation;

/*
 * Content.Goobstation.IntegrationTests 2027
 */

[TestFixture]
public sealed class UplinkPreferenceTests
{
    [Test]
    public async Task TestFindPdaUplinkTarget()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Dirty = true,
            DummyTicker = false,
            Connected = true,
            InLobby = true,
        });

        var server = pair.Server;
        var entMan = server.EntMan;
        var uplinkSys = server.System<UplinkSystem>();
        var ticker = server.System<GameTicker>();

        await server.WaitPost(() =>
        {
            ticker.ToggleReadyAll(true);
            ticker.StartRound();
        });
        await pair.RunTicksSync(10);

        var player = pair.Player!.AttachedEntity!.Value;

        await server.WaitAssertion(() =>
        {
            var pdaTarget = uplinkSys.FindPdaUplinkTarget(player);
            Assert.That(pdaTarget, Is.Not.Null, "Player should have a PDA");
            Assert.That(entMan.HasComponent<PdaComponent>(pdaTarget!.Value), Is.True);
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task TestFindPenUplinkTarget()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Dirty = true,
            DummyTicker = false,
            Connected = true,
            InLobby = true,
        });

        var server = pair.Server;
        var entMan = server.EntMan;
        var uplinkSys = server.System<GoobCommonUplinkSystem>();
        var tagSys = server.System<TagSystem>();
        var handsSys = server.System<SharedHandsSystem>();
        var ticker = server.System<GameTicker>();

        await server.WaitPost(() =>
        {
            ticker.ToggleReadyAll(true);
            ticker.StartRound();
        });
        await pair.RunTicksSync(10);

        var player = pair.Player!.AttachedEntity!.Value;

        EntityUid pen = default;
        await server.WaitPost(() =>
        {
            pen = entMan.SpawnEntity("Pen", entMan.GetComponent<TransformComponent>(player).Coordinates);
            handsSys.TryPickupAnyHand(player, pen);
        });
        await pair.RunTicksSync(5);

        await server.WaitAssertion(() =>
        {
            var penTarget = uplinkSys.FindPenUplinkTarget(player);
            Assert.That(penTarget, Is.Not.Null, "Player should have a pen");
            Assert.That(tagSys.HasTag(penTarget!.Value, "Pen"), Is.True);
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task TestAddUplinkPdaPreference()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Dirty = true,
            DummyTicker = false,
            Connected = true,
            InLobby = true,
        });

        var server = pair.Server;
        var entMan = server.EntMan;
        var uplinkSys = server.System<UplinkSystem>();
        var ticker = server.System<GameTicker>();

        await server.WaitPost(() =>
        {
            ticker.ToggleReadyAll(true);
            ticker.StartRound();
        });
        await pair.RunTicksSync(10);

        var player = pair.Player!.AttachedEntity!.Value;

        await server.WaitAssertion(() =>
        {
            var pdaTarget = uplinkSys.FindPdaUplinkTarget(player);
            Assert.That(pdaTarget, Is.Not.Null, "Player should have a PDA");

            var result = uplinkSys.AddUplink(player, 20, UplinkPreference.Pda);
            Assert.That(result, Is.True);

            Assert.That(entMan.HasComponent<StoreComponent>(pdaTarget!.Value), Is.True);
            var store = entMan.GetComponent<StoreComponent>(pdaTarget.Value);
            Assert.That(store.Balance.ContainsKey("Telecrystal"), Is.True);
            Assert.That((int) store.Balance["Telecrystal"], Is.EqualTo(20));
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task TestAddUplinkPenPreference()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Dirty = true,
            DummyTicker = false,
            Connected = true,
            InLobby = true,
        });

        var server = pair.Server;
        var entMan = server.EntMan;
        var uplinkSys = server.System<UplinkSystem>();
        var goobUplinkSys = server.System<GoobCommonUplinkSystem>();
        var handsSys = server.System<SharedHandsSystem>();
        var ticker = server.System<GameTicker>();

        await server.WaitPost(() =>
        {
            ticker.ToggleReadyAll(true);
            ticker.StartRound();
        });
        await pair.RunTicksSync(10);

        var player = pair.Player!.AttachedEntity!.Value;

        EntityUid pen = default;
        await server.WaitPost(() =>
        {
            pen = entMan.SpawnEntity("Pen", entMan.GetComponent<TransformComponent>(player).Coordinates);
            handsSys.TryPickupAnyHand(player, pen);
        });
        await pair.RunTicksSync(5);

        await server.WaitAssertion(() =>
        {
            var penTarget = goobUplinkSys.FindPenUplinkTarget(player);
            Assert.That(penTarget, Is.Not.Null, "Player should have a pen");

            var result = uplinkSys.AddUplink(player, 20, UplinkPreference.Pen);
            Assert.That(result, Is.True);

            Assert.That(entMan.HasComponent<PenSpinComponent>(penTarget!.Value), Is.True);
            Assert.That(entMan.HasComponent<PenSpinUplinkComponent>(penTarget.Value), Is.True);
            Assert.That(entMan.HasComponent<StoreComponent>(penTarget.Value), Is.True);

            var store = entMan.GetComponent<StoreComponent>(penTarget.Value);
            Assert.That(store.Balance.ContainsKey("Telecrystal"), Is.True);
            Assert.That((int) store.Balance["Telecrystal"], Is.EqualTo(20));
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task TestPenUplinkCodeGeneration()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Dirty = true,
            DummyTicker = false,
            Connected = true,
            InLobby = true,
        });

        var server = pair.Server;
        var entMan = server.EntMan;
        var uplinkSys = server.System<UplinkSystem>();
        var goobUplinkSys = server.System<GoobCommonUplinkSystem>();
        var handsSys = server.System<SharedHandsSystem>();
        var ticker = server.System<GameTicker>();

        await server.WaitPost(() =>
        {
            ticker.ToggleReadyAll(true);
            ticker.StartRound();
        });
        await pair.RunTicksSync(10);

        var player = pair.Player!.AttachedEntity!.Value;

        EntityUid pen = default;
        await server.WaitPost(() =>
        {
            pen = entMan.SpawnEntity("Pen", entMan.GetComponent<TransformComponent>(player).Coordinates);
            handsSys.TryPickupAnyHand(player, pen);
        });
        await pair.RunTicksSync(5);

        await server.WaitPost(() =>
        {
            uplinkSys.AddUplink(player, 20, UplinkPreference.Pen);
        });
        await pair.RunTicksSync(5);

        await server.WaitAssertion(() =>
        {
            var penTarget = goobUplinkSys.FindPenUplinkTarget(player);
            Assert.That(penTarget, Is.Not.Null);

            var spinComp = entMan.GetComponent<PenSpinComponent>(penTarget!.Value);
            Assert.That(spinComp.CombinationLength, Is.EqualTo(4));
            Assert.That(spinComp.MinDegree, Is.EqualTo(0));
            Assert.That(spinComp.MaxDegree, Is.EqualTo(359));

            var ev = new GeneratePenSpinCodeEvent();
            entMan.EventBus.RaiseLocalEvent(penTarget.Value, ref ev);

            var uplinkComp = entMan.GetComponent<PenSpinUplinkComponent>(penTarget.Value);
            Assert.That(uplinkComp.Code, Is.Not.Null);
            Assert.That(uplinkComp.Code!.Length, Is.EqualTo(4));

            foreach (var degree in uplinkComp.Code)
            {
                Assert.That(degree, Is.GreaterThanOrEqualTo(0));
                Assert.That(degree, Is.LessThanOrEqualTo(359));
            }
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task TestAddUplinkImplantPreference()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Dirty = true,
            DummyTicker = false,
            Connected = true,
            InLobby = true,
        });

        var server = pair.Server;
        var uplinkSys = server.System<UplinkSystem>();
        var ticker = server.System<GameTicker>();

        await server.WaitPost(() =>
        {
            ticker.ToggleReadyAll(true);
            ticker.StartRound();
        });
        await pair.RunTicksSync(10);

        var player = pair.Player!.AttachedEntity!.Value;

        await server.WaitAssertion(() =>
        {
            var result = uplinkSys.AddUplink(player, 20, UplinkPreference.Implant);
            Assert.That(result, Is.True);
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task TestAddUplinkFallbackToImplant()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Dirty = true,
        });

        var server = pair.Server;
        var entMan = server.EntMan;
        var uplinkSys = server.System<UplinkSystem>();
        var goobUplinkSys = server.System<GoobCommonUplinkSystem>();

        EntityUid dummy = default;
        await server.WaitPost(() =>
        {
            dummy = entMan.SpawnEntity("MobHuman", MapCoordinates.Nullspace);
        });
        await pair.RunTicksSync(5);

        await server.WaitAssertion(() =>
        {
            var penTarget = goobUplinkSys.FindPenUplinkTarget(dummy);
            Assert.That(penTarget, Is.Null, "Dummy should not have a pen");

            var result = uplinkSys.AddUplink(dummy, 20, UplinkPreference.Pen);
            Assert.That(result, Is.True, "Should fall back to implant when pen unavailable");
        });

        await pair.CleanReturnAsync();
    }
}
