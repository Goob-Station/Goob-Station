#nullable enable
using System.Linq;
using System.Numerics;
using Content.Goobstation.Server.Administration.Objectives;
using Content.Server.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Content.Shared.Objectives.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.IntegrationTests.Tests._Goobstation.Administration;

/// <summary>
/// Tests the logic behind the admin objectives panel: custom, targeted and steal objectives.
/// </summary>
public sealed class AdminObjectivesTest
{
    private const string TargetObjectiveProto = "AdminObjectivesTestKillObjective";
    private const string DummyUsername = "AdminObjectivesTestUser";
    private const string Issuer = "objective-issuer-syndicate";

    [TestPrototypes]
    private const string Prototypes = $"""
- type: entity
  id: {TargetObjectiveProto}
  components:
  - type: Objective
    difficulty: 1
    issuer: {Issuer}
    icon:
      sprite: error.rsi
      state: error
  - type: RoleRequirement
    roles:
    - TraitorRole
  - type: TargetObjective
    title: objective-condition-kill-person-title
  - type: PickRandomPerson
  - type: KillPersonCondition
""";

    [Test]
    public async Task CustomObjectiveTest()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var playerMan = server.ResolveDependency<ISharedPlayerManager>();
        var mindSys = server.System<SharedMindSystem>();
        var adminObjectives = server.System<AdminObjectivesSystem>();
        var objectives = server.System<SharedObjectivesSystem>();

        await server.AddDummySession(DummyUsername);
        await server.WaitRunTicks(5);
        var session = playerMan.Sessions.Single();

        await server.WaitAssertion(() =>
        {
            var mind = mindSys.CreateMind(session.UserId);

            const string text = "Do the [b]thing[/b]";
            Assert.That(adminObjectives.TryAddCustom(session, mind, text, "A description", Issuer, out _), Is.True);

            var objective = mind.Comp.Objectives.Single();
            var info = objectives.GetInfo(objective, mind, mind.Comp);
            Assert.That(info, Is.Not.Null);
            Assert.That(info!.Value.Progress, Is.EqualTo(1f), "Custom objectives are always complete.");
            Assert.That(info.Value.Title, Is.EqualTo(FormattedMessage.EscapeText(text)), "Markup in the title must be escaped.");
            Assert.That(info.Value.Description, Is.EqualTo("A description"));
            Assert.That(server.EntMan.GetComponent<ObjectiveComponent>(objective).Issuer.Id, Is.EqualTo(Issuer));

            Assert.That(adminObjectives.TryAddCustom(session, mind, "   ", "", Issuer, out _), Is.False, "Empty titles must be rejected.");
            Assert.That(mind.Comp.Objectives, Has.Count.EqualTo(1));

            Assert.That(adminObjectives.TryRemove(session, mind, objective, out _), Is.True);
            Assert.That(mind.Comp.Objectives, Is.Empty);
            Assert.That(server.EntMan.Deleted(objective), Is.True, "The removed objective should be deleted.");
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task TargetedPrototypeObjectiveTest()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var entMan = server.EntMan;
        var playerMan = server.ResolveDependency<ISharedPlayerManager>();
        var mindSys = server.System<SharedMindSystem>();
        var adminObjectives = server.System<AdminObjectivesSystem>();
        var targetSys = server.System<TargetObjectiveSystem>();

        await server.AddDummySession(DummyUsername);
        await server.WaitRunTicks(5);
        var session = playerMan.Sessions.Single();

        await server.WaitAssertion(() =>
        {
            var mind = mindSys.CreateMind(session.UserId);
            var targetMind = mindSys.CreateMind(null, "Target Person");

            // the traitor role requirement fails
            Assert.That(adminObjectives.TryAddPrototype(session, mind, TargetObjectiveProto, targetMind, false, out _), Is.False);
            Assert.That(mind.Comp.Objectives, Is.Empty);

            var leaked = 0;
            var query = entMan.AllEntityQueryEnumerator<ObjectiveComponent>();
            while (query.MoveNext(out _, out _))
            {
                leaked++;
            }

            Assert.That(leaked, Is.Zero, "A failed requirement check must not leak the spawned objective.");

            // bypassing it works, and the designated target is used instead of a random one
            Assert.That(adminObjectives.TryAddPrototype(session, mind, TargetObjectiveProto, targetMind, true, out _), Is.True);

            var objective = mind.Comp.Objectives.Single();
            Assert.That(targetSys.GetTarget(objective, out var target), Is.True);
            Assert.That(target, Is.EqualTo(targetMind.Owner));
            Assert.That(entMan.GetComponent<MetaDataComponent>(objective).EntityName, Does.Contain("Target Person"));
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task StealSpecificEntityTest()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var entMan = server.EntMan;
        var playerMan = server.ResolveDependency<ISharedPlayerManager>();
        var mindSys = server.System<SharedMindSystem>();
        var adminObjectives = server.System<AdminObjectivesSystem>();
        var objectives = server.System<SharedObjectivesSystem>();
        var hands = server.System<SharedHandsSystem>();
        var testMap = await pair.CreateTestMap();

        await server.AddDummySession(DummyUsername);
        await server.WaitRunTicks(5);
        var session = playerMan.Sessions.Single();

        await server.WaitAssertion(() =>
        {
            var mind = mindSys.CreateMind(session.UserId);
            var body = entMan.SpawnEntity("MobHuman", testMap.MapCoords);
            mindSys.TransferTo(mind, body);

            var item = entMan.SpawnEntity("Crowbar", testMap.MapCoords);

            Assert.That(adminObjectives.TryAddSteal(session, mind, body, Issuer, out _), Is.False, "Can't steal your own body.");
            Assert.That(adminObjectives.TryAddSteal(session, mind, item, Issuer, out _), Is.True);

            var objective = mind.Comp.Objectives.Single();
            var itemName = entMan.GetComponent<MetaDataComponent>(item).EntityName;
            Assert.That(entMan.GetComponent<MetaDataComponent>(objective).EntityName, Does.Contain(itemName));
            Assert.That(objectives.GetProgress(objective, mind), Is.EqualTo(0f), "Not stolen yet.");

            Assert.That(hands.TryPickupAnyHand(body, item), Is.True);
            Assert.That(objectives.GetProgress(objective, mind), Is.EqualTo(1f), "Holding the entity completes the objective.");

            hands.TryDrop(body, item);
            Assert.That(objectives.GetProgress(objective, mind), Is.EqualTo(0f));

            // a deleted target can't be stolen anymore
            entMan.DeleteEntity(item);
            Assert.That(objectives.GetProgress(objective, mind), Is.EqualTo(0f));
        });

        await pair.CleanReturnAsync();
    }

    /// <summary>
    /// Like normal steal objectives, admin steal objectives count the entity when it is in range of a steal area
    /// (e.g. a thief beacon) that the mind has linked.
    /// </summary>
    [Test]
    public async Task StealSpecificEntityBeaconTest()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var entMan = server.EntMan;
        var playerMan = server.ResolveDependency<ISharedPlayerManager>();
        var mindSys = server.System<SharedMindSystem>();
        var adminObjectives = server.System<AdminObjectivesSystem>();
        var objectives = server.System<SharedObjectivesSystem>();
        var xforms = server.System<SharedTransformSystem>();
        var testMap = await pair.CreateTestMap();

        await server.AddDummySession(DummyUsername);
        await server.WaitRunTicks(5);
        var session = playerMan.Sessions.Single();

        await server.WaitAssertion(() =>
        {
            // no body on purpose, the beacon must work without one
            var mind = mindSys.CreateMind(session.UserId);
            var otherMind = mindSys.CreateMind(null, "Someone Else");

            var beacon = entMan.SpawnEntity(null, testMap.MapCoords);
            var area = entMan.AddComponent<StealAreaComponent>(beacon);
            area.Range = 1f;

            var item = entMan.SpawnEntity("Crowbar", testMap.MapCoords);
            Assert.That(adminObjectives.TryAddSteal(session, mind, item, Issuer, out _), Is.True);
            var objective = mind.Comp.Objectives.Single();

            Assert.That(objectives.GetProgress(objective, mind), Is.EqualTo(0f), "The beacon isn't linked to the mind yet.");

            area.Owners.Add(mind);
            Assert.That(objectives.GetProgress(objective, mind), Is.EqualTo(1f), "The item is in range of the mind's beacon.");

            xforms.SetWorldPosition(item, xforms.GetWorldPosition(beacon) + new Vector2(10f, 0f));
            Assert.That(objectives.GetProgress(objective, mind), Is.EqualTo(0f), "The item is out of range of the beacon.");

            xforms.SetWorldPosition(item, xforms.GetWorldPosition(beacon));
            area.Owners.Clear();
            area.Owners.Add(otherMind);
            Assert.That(objectives.GetProgress(objective, mind), Is.EqualTo(0f), "The beacon belongs to someone else.");
        });

        await pair.CleanReturnAsync();
    }
}
