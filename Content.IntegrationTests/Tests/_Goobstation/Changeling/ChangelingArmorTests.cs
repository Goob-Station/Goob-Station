using System.Linq;
using System.Runtime.CompilerServices;
using Content.Server.Actions;
using Content.Server.Antag;
using Content.Server.Changeling;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Mind;
using Content.Shared.Actions;
using Content.Shared.Changeling;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Inventory;
using Robust.Shared.GameObjects;
using Robust.Shared.Timing;

namespace Content.IntegrationTests.Tests._Goobstation.Changeling;

[TestFixture]
public sealed class ChangelingArmorTest
{
    [Test]
    public async Task TestChangelingChitinousArmor()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Dirty = true,
            InLobby = false,
            DummyTicker = false,
        });

        var server = pair.Server;
        var testMap = await pair.CreateTestMap();
        var ticker = server.System<GameTicker>();
        var entMan = server.ResolveDependency<IEntityManager>();
        var timing = server.ResolveDependency<IGameTiming>();

        var lingSys = entMan.System<ChangelingSystem>();
        var antagSys = entMan.System<AntagSelectionSystem>();
        var mindSys = entMan.System<MindSystem>();
        var actionSys = entMan.System<ActionsSystem>();
        var invSys = entMan.System<InventorySystem>();

        Assert.That(ticker.RunLevel, Is.EqualTo(GameRunLevel.InRound));

        await server.WaitAssertion(() =>
        {
            // Spawn a urist
            var urist = entMan.SpawnEntity("MobHuman", testMap.GridCoords);
            Assert.That(urist.IsValid());

            // Make urist a changeling
            var changeling = entMan.AddComponent<ChangelingComponent>(urist);
            changeling.TotalAbsorbedEntities += 10;
            changeling.MaxChemicals = 1000;
            changeling.Chemicals = 1000;

            server.RunTicks(5);

            // Give urist chitinous armor action
            var armorActionEnt = actionSys.AddAction(urist, "ActionToggleChitinousArmor");
            Assert.That(armorActionEnt, Is.Not.Null);
            Entity<InstantActionComponent> armorAction = (armorActionEnt.Value, entMan.GetComponent<InstantActionComponent>(armorActionEnt.Value));
            ChangelingActionComponent lingAction = entMan.GetComponent<ChangelingActionComponent>(armorActionEnt.Value);

            // Armor up
            actionSys.PerformAction(urist, null, armorAction, armorAction.Comp, armorAction.Comp.BaseEvent, timing.CurTime);

            server.RunTicks(5);

            Assert.That(invSys.TryGetSlotEntity(urist, "outerClothing", out var outerClothing));
            Assert.That(outerClothing, Is.Not.Null);
            Assert.That(entMan.GetComponent<MetaDataComponent>(outerClothing.Value).EntityPrototype!.ID, Is.EqualTo(lingSys.ArmorPrototype.Id));

            Assert.That(invSys.TryGetSlotEntity(urist, "head", out var head));
            Assert.That(head, Is.Not.Null);
            Assert.That(entMan.GetComponent<MetaDataComponent>(head.Value).EntityPrototype!.ID, Is.EqualTo(lingSys.ArmorHelmetPrototype.Id));
        });

        await pair.CleanReturnAsync();
    }
}
