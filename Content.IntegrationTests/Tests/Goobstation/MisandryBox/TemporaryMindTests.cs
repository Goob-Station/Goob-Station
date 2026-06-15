using System.Linq;
using Content.Goobstation.Server.MisandryBox.Mind;
using Content.Goobstation.Shared.MisandryBox.Mind;
using Content.IntegrationTests.Pair;
using Content.Server.GameTicking;
using Content.Shared.Ghost;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Robust.Server.Player;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Player;

namespace Content.IntegrationTests.Tests.Goobstation.MisandryBox;

[TestFixture]
public sealed class TemporaryMindTests
{
    [Test]
    public async Task SwapAndRestoreAsGhost()
    {
        await using var pair = await SetupPair();
        var ctx = await SetupContext(pair);

        await pair.Server.WaitPost(() =>
        {
            Assert.That(ctx.TempMindSys.TrySwapTempMind(ctx.Player, ctx.NewBody), Is.True);
        });
        await pair.RunTicksSync(5);

        EntityUid disposableMind = default;

        await pair.Server.WaitAssertion(() =>
        {
            Assert.That(ctx.Player.AttachedEntity, Is.EqualTo(ctx.NewBody));

            var temp = ctx.EntMan.GetComponent<TemporaryMindComponent>(ctx.NewBody);
            Assert.That(temp.OriginalMind, Is.EqualTo(ctx.OrigMindId));
            disposableMind = temp.DisposableMind;

            var origMind = ctx.EntMan.GetComponent<MindComponent>(ctx.OrigMindId);
            Assert.That(origMind.UserId, Is.Null, "Original mind userId should be null during swap");

            var dispMind = ctx.EntMan.GetComponent<MindComponent>(disposableMind);
            Assert.That(dispMind.UserId, Is.EqualTo(ctx.Player.UserId));
            Assert.That(ctx.MindSys.GetMind(ctx.Player.UserId), Is.EqualTo(disposableMind));
        });

        EntityUid? ghost = null;
        await pair.Server.WaitPost(() =>
        {
            ghost = ctx.TempMindSys.TryRestoreAsGhost(ctx.NewBody);
        });
        await pair.RunTicksSync(5);

        await pair.Server.WaitAssertion(() =>
        {
            Assert.That(ghost, Is.Not.Null, "TryRestoreAsGhost should return a ghost");
            Assert.That(ctx.EntMan.HasComponent<GhostComponent>(ghost!.Value));
            Assert.That(ctx.Player.AttachedEntity, Is.EqualTo(ghost!.Value));

            var origMind = ctx.EntMan.GetComponent<MindComponent>(ctx.OrigMindId);
            Assert.That(origMind.UserId, Is.EqualTo(ctx.Player.UserId));
            Assert.That(origMind.VisitingEntity, Is.EqualTo(ghost!.Value));
            Assert.That(origMind.OwnedEntity, Is.EqualTo(ctx.OriginalBody));

            Assert.That(ctx.MindSys.GetMind(ctx.Player.UserId), Is.EqualTo(ctx.OrigMindId));
            Assert.That(ctx.EntMan.Deleted(disposableMind) || ctx.EntMan.IsQueuedForDeletion(disposableMind));
            Assert.That(!ctx.EntMan.HasComponent<TemporaryMindComponent>(ctx.NewBody));
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task SwapAndRestoreToOriginalBody()
    {
        await using var pair = await SetupPair();
        var ctx = await SetupContext(pair);

        await pair.Server.WaitPost(() =>
        {
            Assert.That(ctx.TempMindSys.TrySwapTempMind(ctx.Player, ctx.NewBody), Is.True);
        });
        await pair.RunTicksSync(5);

        await pair.Server.WaitPost(() =>
        {
            Assert.That(ctx.TempMindSys.TryRestoreToOriginalBody(ctx.NewBody), Is.True);
        });
        await pair.RunTicksSync(5);

        await pair.Server.WaitAssertion(() =>
        {
            Assert.That(ctx.Player.AttachedEntity, Is.EqualTo(ctx.OriginalBody));

            var origMind = ctx.EntMan.GetComponent<MindComponent>(ctx.OrigMindId);
            Assert.That(origMind.UserId, Is.EqualTo(ctx.Player.UserId));
            Assert.That(origMind.OwnedEntity, Is.EqualTo(ctx.OriginalBody));

            Assert.That(ctx.MindSys.GetMind(ctx.Player.UserId), Is.EqualTo(ctx.OrigMindId));
            Assert.That(!ctx.EntMan.HasComponent<TemporaryMindComponent>(ctx.NewBody));
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task OriginalBodyDeletedFallsBackToGhost()
    {
        await using var pair = await SetupPair();
        var ctx = await SetupContext(pair);

        await pair.Server.WaitPost(() =>
        {
            Assert.That(ctx.TempMindSys.TrySwapTempMind(ctx.Player, ctx.NewBody), Is.True);
        });
        await pair.RunTicksSync(5);

        await pair.Server.WaitPost(() =>
        {
            ctx.EntMan.DeleteEntity(ctx.OriginalBody);
        });
        await pair.RunTicksSync(5);

        await pair.Server.WaitAssertion(() =>
        {
            Assert.That(ctx.TempMindSys.TryRestoreToOriginalBody(ctx.NewBody), Is.False,
                "TryRestoreToOriginalBody should fail when original body is deleted");
        });

        EntityUid? ghost = null;
        await pair.Server.WaitPost(() =>
        {
            ghost = ctx.TempMindSys.TryRestoreAsGhost(ctx.NewBody);
        });
        await pair.RunTicksSync(5);

        await pair.Server.WaitAssertion(() =>
        {
            Assert.That(ghost, Is.Not.Null, "TryRestoreAsGhost should still succeed");
            Assert.That(ctx.EntMan.HasComponent<GhostComponent>(ghost!.Value));
            Assert.That(ctx.Player.AttachedEntity, Is.EqualTo(ghost!.Value));

            var origMind = ctx.EntMan.GetComponent<MindComponent>(ctx.OrigMindId);
            Assert.That(origMind.UserId, Is.EqualTo(ctx.Player.UserId));
            Assert.That(ctx.MindSys.GetMind(ctx.Player.UserId), Is.EqualTo(ctx.OrigMindId));
        });

        await pair.CleanReturnAsync();
    }

    private static async Task<TestPair> SetupPair()
    {
        var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            DummyTicker = false,
            Connected = true,
            Dirty = true,
            InLobby = true,
        });

        var ticker = pair.Server.System<GameTicker>();
        await pair.Server.WaitPost(() =>
        {
            ticker.ToggleReadyAll(true);
            ticker.StartRound();
        });
        await pair.RunTicksSync(10);

        return pair;
    }

    private static async Task<TestContext> SetupContext(TestPair pair)
    {
        var server = pair.Server;
        var entMan = server.EntMan;
        var mindSys = server.System<SharedMindSystem>();
        var tempMindSys = server.System<TemporaryMindSystem>();
        var playerMan = server.ResolveDependency<IPlayerManager>();
        var player = playerMan.Sessions.Single();

        var ctx = new TestContext
        {
            EntMan = entMan,
            MindSys = mindSys,
            TempMindSys = tempMindSys,
            Player = player,
        };

        await server.WaitPost(() =>
        {
            // Use the player's existing mind from the round start
            mindSys.TryGetMind(player, out var origMindId, out _);
            ctx.OrigMindId = origMindId;

            ctx.OriginalBody = entMan.SpawnEntity(null, MapCoordinates.Nullspace);
            entMan.EnsureComponent<MindContainerComponent>(ctx.OriginalBody);
            mindSys.SetGhostOnShutdown(ctx.OriginalBody, false);
            mindSys.TransferTo(ctx.OrigMindId, ctx.OriginalBody);
            playerMan.SetAttachedEntity(player, ctx.OriginalBody);

            ctx.NewBody = entMan.SpawnEntity(null, MapCoordinates.Nullspace);
            entMan.EnsureComponent<MindContainerComponent>(ctx.NewBody);
        });

        await pair.RunTicksSync(5);
        return ctx;
    }

    private sealed class TestContext
    {
        public IEntityManager EntMan = default!;
        public SharedMindSystem MindSys = default!;
        public TemporaryMindSystem TempMindSys = default!;
        public ICommonSession Player = default!;
        public EntityUid OriginalBody;
        public EntityUid NewBody;
        public EntityUid OrigMindId;
    }
}
