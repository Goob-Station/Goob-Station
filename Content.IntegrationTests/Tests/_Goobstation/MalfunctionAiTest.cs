using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Server.MalfunctionAi;
using Content.Server.Store.Systems;
using Content.Goobstation.Shared.MalfunctionAi;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Robust.Shared.GameObjects;

namespace Content.IntegrationTests.Tests._Goobstation;

[TestFixture]
public sealed class MalfunctionAiTest
{
    /// <summary>
    /// The Malfunction AI store must be created with the starting CPU balance,
    /// and hacking an APC must grant CpuPerApc (each APC only once).
    /// </summary>
    [Test]
    public async Task ApcHackGrantsCpu()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var testMap = await pair.CreateTestMap();
        var entMan = server.ResolveDependency<IEntityManager>();
        var coordinates = testMap.GridCoords;

        await server.WaitAssertion(() =>
        {
            var malfSystem = entMan.System<MalfunctionAiSystem>();

            var ai = entMan.SpawnEntity(null, coordinates);
            var malf = entMan.AddComponent<MalfunctionAiComponent>(ai);

            Assert.That(entMan.TryGetComponent<StoreComponent>(ai, out var store), Is.True,
                "MalfunctionAiComponent did not set up a store");
            Assert.That(store!.Balance[malf.Currency], Is.EqualTo(malf.StartingPower),
                "Starting CPU balance is wrong");

            var apc = entMan.SpawnEntity("APCBasic", coordinates);
            Assert.That(malfSystem.TryHackApc((ai, malf), apc), Is.True, "APC hack failed");
            Assert.That(store.Balance[malf.Currency], Is.EqualTo(malf.StartingPower + malf.CpuPerApc),
                "APC hack should grant CpuPerApc CPU");
            Assert.That(malf.HackedApcCount, Is.EqualTo(1));
            Assert.That(malf.ProcessingPower, Is.EqualTo(store.Balance[malf.Currency]),
                "HUD mirror out of sync with store balance");

            // The same APC cannot be hacked twice.
            Assert.That(malfSystem.TryHackApc((ai, malf), apc), Is.False);
            Assert.That(store.Balance[malf.Currency], Is.EqualTo(malf.StartingPower + malf.CpuPerApc));
            Assert.That(malf.HackedApcCount, Is.EqualTo(1));
        });

        await pair.CleanReturnAsync();
    }

    /// <summary>
    /// The malf ability store UI must actually open for the AI player. The store BUI is registered
    /// at runtime and the AI fails regular interaction checks, so this guards the whole open path
    /// (runtime InterfaceData registration + requireInputValidation off).
    /// </summary>
    [Test]
    public async Task StoreUiOpens()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            DummyTicker = false,
            Connected = true,
            Dirty = true,
        });
        var server = pair.Server;
        var testMap = await pair.CreateTestMap();
        var entMan = server.ResolveDependency<IEntityManager>();
        var coordinates = testMap.GridCoords;

        var clientSession = pair.Client.Session!;
        var serverSession = server.PlayerMan.GetSessionById(clientSession.UserId);

        EntityUid ai = default;
        await server.WaitPost(() =>
        {
            ai = entMan.SpawnEntity(null, coordinates);
            entMan.AddComponent<MalfunctionAiComponent>(ai);
            server.PlayerMan.SetAttachedEntity(serverSession, ai);
        });

        await pair.RunTicksSync(5);

        await server.WaitAssertion(() =>
        {
            var storeSystem = entMan.System<StoreSystem>();
            var uiSystem = entMan.System<SharedUserInterfaceSystem>();

            storeSystem.ToggleUi(ai, ai);
            Assert.That(uiSystem.IsUiOpen(ai, StoreUiKey.Key), Is.True,
                "The malf store UI did not open for the AI actor");
        });

        // Let the client actually receive the BUI state and construct the store window,
        // so client-side crashes on opening the store surface in this test.
        await pair.RunTicksSync(15);

        await pair.CleanReturnAsync();
    }
}
