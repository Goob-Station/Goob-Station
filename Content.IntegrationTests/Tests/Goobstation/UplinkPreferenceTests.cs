using Content.Goobstation.Common.Traitor;
using Content.Goobstation.Server.Traitor.PenSpin;
using Content.Goobstation.Shared.Traitor.PenSpin;
using Content.IntegrationTests.Pair;
using Content.Server.GameTicking;
using Content.Server.Traitor.Uplink;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Implants.Components;
using Content.Shared.PDA;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests.Goobstation;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public sealed class UplinkPreferenceTests
{
    private TestPair _pair = default!;
    private EntityUid _player;

    private static readonly ProtoId<UplinkPreferencePrototype> PenPreference = "UplinkPen";

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
        var goobUplinkSys = server.System<GoobCommonUplinkSystem>();

        await server.WaitAssertion(() =>
        {
            var pdaTarget = goobUplinkSys.FindUplinkTarget(_player, new[] { "Pda" });
            Assert.That(pdaTarget, Is.Not.Null, "Player should have a PDA");
            Assert.That(entMan.HasComponent<PdaComponent>(pdaTarget!.Value), Is.True);
        });
    }

    [Test]
    public async Task TestFindPenUplinkTarget()
    {
        var server = _pair.Server;
        var entMan = server.EntMan;
        var goobUplinkSys = server.System<GoobCommonUplinkSystem>();

        await SpawnPenInHand();

        await server.WaitAssertion(() =>
        {
            var penTarget = goobUplinkSys.FindUplinkTarget(_player, new[] { "Pen" });
            Assert.That(penTarget, Is.Not.Null, "Player should have a pen");
            Assert.That(entMan.HasComponent<PenComponent>(penTarget!.Value), Is.True);
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

        await server.WaitPost(() => uplinkSys.TryAddUplink(_player, 20, PenPreference, out _, out _));
        await _pair.RunTicksSync(5);

        await server.WaitAssertion(() =>
        {
            var penTarget = goobUplinkSys.FindUplinkTarget(_player, new[] { "Pen" });
            Assert.That(penTarget, Is.Not.Null);

            var spinComp = entMan.GetComponent<PenComponent>(penTarget!.Value);
            Assert.That(spinComp.CombinationLength, Is.EqualTo(4));
            Assert.That(spinComp.MinDegree, Is.EqualTo(0));
            Assert.That(spinComp.MaxDegree, Is.EqualTo(359));

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
    public async Task TestAllUplinkPreferences()
    {
        var server = _pair.Server;
        var entMan = server.EntMan;
        var protoMan = server.ProtoMan;
        var uplinkSys = server.System<UplinkSystem>();
        var handsSys = server.System<SharedHandsSystem>();

        foreach (var pref in protoMan.EnumeratePrototypes<UplinkPreferencePrototype>())
        {
            if (pref.SearchComponents != null)
            {
                await server.WaitPost(() =>
                {
                    if (Array.IndexOf(pref.SearchComponents, "Pen") >= 0)
                    {
                        var coords = entMan.GetComponent<TransformComponent>(_player).Coordinates;
                        var pen = entMan.SpawnEntity("Pen", coords);
                        handsSys.TryPickupAnyHand(_player, pen);
                    }
                });
                await _pair.RunTicksSync(5);
            }

            await server.WaitAssertion(() =>
            {
                var success = uplinkSys.TryAddUplink(_player, 20, pref.ID, out var uplinkTarget, out var setupEvent);
                Assert.That(success, Is.True, $"TryAddUplink failed for preference {pref.ID}");

                if (pref.SearchComponents != null)
                {
                    Assert.That(uplinkTarget, Is.Not.Null, $"Should find uplink target for {pref.ID}");
                    Assert.That(setupEvent, Is.Not.Null, $"SetupUplinkEvent should be raised for {pref.ID}");
                    Assert.That(setupEvent!.Value.Handled, Is.True, $"SetupUplinkEvent should be handled for {pref.ID}");

                    var store = entMan.GetComponent<StoreComponent>(uplinkTarget!.Value);
                    Assert.That(store.Balance.ContainsKey("Telecrystal"), Is.True, $"Store should have TC for {pref.ID}");
                    Assert.That((int) store.Balance["Telecrystal"], Is.EqualTo(20), $"Store should have 20 TC for {pref.ID}");
                }
                else // Fallback
                {
                    Assert.That(uplinkTarget, Is.Null, $"Implant preference {pref.ID} should have no target entity");
                    Assert.That(setupEvent, Is.Null, $"Implant preference {pref.ID} should have no setup event");
                }
            });
        }
    }

    [Test]
    public async Task TestImplantUplinkBalance()
    {
        var server = _pair.Server;
        var entMan = server.EntMan;
        var protoMan = server.ProtoMan;
        var uplinkSys = server.System<UplinkSystem>();

        const int startingBalance = 100;
        var implantPreference = new ProtoId<UplinkPreferencePrototype>("UplinkImplant");

        await server.WaitAssertion(() =>
        {
            var success = uplinkSys.TryAddUplink(_player, startingBalance, implantPreference, out var uplinkTarget, out _);
            Assert.That(success, Is.True, "Implant uplink should succeed");
            Assert.That(uplinkTarget, Is.Null, "Implant preference should not return an uplink target entity");

            Assert.That(entMan.TryGetComponent<ImplantedComponent>(_player, out var implanted), Is.True,
                "Player should have ImplantedComponent after implant uplink");

            EntityUid? implantStore = null;
            foreach (var implantEnt in implanted!.ImplantContainer.ContainedEntities)
            {
                if (entMan.HasComponent<StoreComponent>(implantEnt))
                {
                    implantStore = implantEnt;
                    break;
                }
            }

            Assert.That(implantStore, Is.Not.Null, "Should find a store component on the implant");

            var store = entMan.GetComponent<StoreComponent>(implantStore!.Value);
            Assert.That(store.Balance.ContainsKey("Telecrystal"), Is.True);

            var catalog = protoMan.Index<ListingPrototype>("UplinkUplinkImplanter");
            var implantCost = (int) catalog.Cost["Telecrystal"];
            var expectedBalance = startingBalance - implantCost;
            Assert.That((int) store.Balance["Telecrystal"], Is.EqualTo(expectedBalance),
                $"Implant store should have {expectedBalance} TC (starting {startingBalance} minus {implantCost} implant cost)");
        });
    }
}

[TestFixture]
public sealed class UplinkFallbackTests
{
    private static readonly ProtoId<UplinkPreferencePrototype> PenPreference = "UplinkPen";

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
            var penTarget = goobUplinkSys.FindUplinkTarget(dummy, new[] { "Pen" });
            Assert.That(penTarget, Is.Null, "Dummy should not have a pen");

            var success = uplinkSys.TryAddUplink(dummy, 20, PenPreference, out _, out _);
            Assert.That(success, Is.True, "Should fall back to implant when pen unavailable");
        });

        await pair.CleanReturnAsync();
    }
}
