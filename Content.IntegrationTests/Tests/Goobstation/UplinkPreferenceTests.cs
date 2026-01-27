using Content.Goobstation.Common.Traitor;
using Content.Goobstation.Server.Traitor.PenSpin;
using Content.Goobstation.Shared.Traitor.PenSpin;
using Content.IntegrationTests.Pair;
using Content.Server.GameTicking;
using Content.Server.Traitor.Uplink;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.PDA;
using Content.Shared.Store.Components;
using Content.Shared.Tag;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;

namespace Content.IntegrationTests.Tests.Goobstation;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class UplinkPreferenceTests
{
    private TestPair _pair = default!;
    private EntityUid _player;

    [SetUp]
    public async Task Setup()
    {
        _pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Dirty = true,
            DummyTicker = false,
            Connected = true,
            InLobby = true,
        });

        var server = _pair.Server;
        var ticker = server.System<GameTicker>();

        await server.WaitPost(() =>
        {
            ticker.ToggleReadyAll(true);
            ticker.StartRound();
        });
        await _pair.RunTicksSync(10);

        _player = _pair.Player!.AttachedEntity!.Value;
    }

    [TearDown]
    public async Task TearDown()
    {
        await _pair.CleanReturnAsync();
    }

    private async Task<EntityUid> SpawnPenInHand()
    {
        var server = _pair.Server;
        var entMan = server.EntMan;
        var handsSys = server.System<SharedHandsSystem>();

        EntityUid pen = default;
        await server.WaitPost(() =>
        {
            var coords = entMan.GetComponent<TransformComponent>(_player).Coordinates;
            pen = entMan.SpawnEntity("Pen", coords);
            handsSys.TryPickupAnyHand(_player, pen);
        });
        await _pair.RunTicksSync(5);

        return pen;
    }

    [Test]
    public async Task TestFindPdaUplinkTarget()
    {
        var server = _pair.Server;
        var entMan = server.EntMan;
        var uplinkSys = server.System<UplinkSystem>();

        await server.WaitAssertion(() =>
        {
            var pdaTarget = uplinkSys.FindPdaUplinkTarget(_player);
            Assert.That(pdaTarget, Is.Not.Null, "Player should have a PDA");
            Assert.That(entMan.HasComponent<PdaComponent>(pdaTarget!.Value), Is.True);
        });
    }

    [Test]
    public async Task TestFindPenUplinkTarget()
    {
        var server = _pair.Server;
        var goobUplinkSys = server.System<GoobCommonUplinkSystem>();
        var tagSys = server.System<TagSystem>();

        await SpawnPenInHand();

        await server.WaitAssertion(() =>
        {
            var penTarget = goobUplinkSys.FindPenUplinkTarget(_player);
            Assert.That(penTarget, Is.Not.Null, "Player should have a pen");
            Assert.That(tagSys.HasTag(penTarget!.Value, "Pen"), Is.True);
        });
    }

    [Test]
    public async Task TestAddUplinkPdaPreference()
    {
        var server = _pair.Server;
        var entMan = server.EntMan;
        var uplinkSys = server.System<UplinkSystem>();

        await server.WaitAssertion(() =>
        {
            var pdaTarget = uplinkSys.FindPdaUplinkTarget(_player);
            Assert.That(pdaTarget, Is.Not.Null, "Player should have a PDA");

            var result = uplinkSys.AddUplink(_player, 20, UplinkPreference.Pda);
            Assert.That(result, Is.True);

            Assert.That(entMan.HasComponent<StoreComponent>(pdaTarget!.Value), Is.True);
            var store = entMan.GetComponent<StoreComponent>(pdaTarget.Value);
            Assert.That(store.Balance.ContainsKey("Telecrystal"), Is.True);
            Assert.That((int) store.Balance["Telecrystal"], Is.EqualTo(20));
        });
    }

    [Test]
    public async Task TestAddUplinkPenPreference()
    {
        var server = _pair.Server;
        var entMan = server.EntMan;
        var uplinkSys = server.System<UplinkSystem>();
        var goobUplinkSys = server.System<GoobCommonUplinkSystem>();

        await SpawnPenInHand();

        await server.WaitAssertion(() =>
        {
            var penTarget = goobUplinkSys.FindPenUplinkTarget(_player);
            Assert.That(penTarget, Is.Not.Null, "Player should have a pen");

            var result = uplinkSys.AddUplink(_player, 20, UplinkPreference.Pen);
            Assert.That(result, Is.True);

            Assert.That(entMan.HasComponent<PenSpinComponent>(penTarget!.Value), Is.True);
            Assert.That(entMan.HasComponent<PenSpinUplinkComponent>(penTarget.Value), Is.True);
            Assert.That(entMan.HasComponent<StoreComponent>(penTarget.Value), Is.True);

            var store = entMan.GetComponent<StoreComponent>(penTarget.Value);
            Assert.That(store.Balance.ContainsKey("Telecrystal"), Is.True);
            Assert.That((int) store.Balance["Telecrystal"], Is.EqualTo(20));
        });
    }

    [Test]
    public async Task TestPenUplinkCodeGeneration()
    {
        var server = _pair.Server;
        var entMan = server.EntMan;
        var uplinkSys = server.System<UplinkSystem>();
        var goobUplinkSys = server.System<GoobCommonUplinkSystem>();

        await SpawnPenInHand();

        await server.WaitPost(() => uplinkSys.AddUplink(_player, 20, UplinkPreference.Pen));
        await _pair.RunTicksSync(5);

        await server.WaitAssertion(() =>
        {
            var penTarget = goobUplinkSys.FindPenUplinkTarget(_player);
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
    }

    [Test]
    public async Task TestAddUplinkImplantPreference()
    {
        var server = _pair.Server;
        var uplinkSys = server.System<UplinkSystem>();

        await server.WaitAssertion(() =>
        {
            var result = uplinkSys.AddUplink(_player, 20, UplinkPreference.Implant);
            Assert.That(result, Is.True);
        });
    }
}

[TestFixture]
public sealed class UplinkFallbackTests
{
    [Test]
    public async Task TestAddUplinkFallbackToImplant()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Dirty = true });
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
