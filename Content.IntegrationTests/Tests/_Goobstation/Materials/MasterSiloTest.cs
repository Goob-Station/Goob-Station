// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Materials;
using Content.Server.Power.Components;
using Content.Shared.Materials;
using Content.Shared.Materials.OreSilo;
using Content.Shared.Power.EntitySystems;
using Robust.Shared.GameObjects;
using Robust.UnitTesting;

namespace Content.IntegrationTests.Tests._Goobstation.Materials;

[TestFixture]
public sealed class MasterSiloTest
{
    private const string Master = "MachineMaterialSiloMaster";
    private const string Department = "MachineMaterialSilo";
    private const string Lathe = "Autolathe";
    private const string Steel = "Steel";

    [Test]
    public async Task LinkRules()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var map = await pair.CreateTestMap();

        var entMan = server.EntMan;
        var silos = server.System<SharedOreSiloSystem>();
        var storage = server.System<SharedMaterialStorageSystem>();

        await server.WaitAssertion(() =>
        {
            var master = Spawn(server, map, Master);
            var otherMaster = Spawn(server, map, Master);
            var dept = Spawn(server, map, Department);
            var otherDept = Spawn(server, map, Department);
            var lathe = Spawn(server, map, Lathe);

            var masterComp = entMan.GetComponent<OreSiloComponent>(master);
            var otherMasterComp = entMan.GetComponent<OreSiloComponent>(otherMaster);
            var deptComp = entMan.GetComponent<OreSiloComponent>(dept);
            var otherDeptComp = entMan.GetComponent<OreSiloComponent>(otherDept);

            Assert.Multiple(() =>
            {
                Assert.That(silos.IsValidClient((master, masterComp), dept), Is.True);
                Assert.That(silos.IsValidClient((master, masterComp), lathe), Is.False);
                Assert.That(silos.IsValidClient((master, masterComp), otherMaster), Is.False);
                Assert.That(silos.IsValidClient((master, masterComp), master), Is.False);
                Assert.That(silos.IsValidClient((dept, deptComp), lathe), Is.True);
                Assert.That(silos.IsValidClient((dept, deptComp), otherDept), Is.False);
                Assert.That(silos.IsValidClient((dept, deptComp), master), Is.False);
                Assert.That(silos.IsValidClient((dept, deptComp), dept), Is.False);
            });

            Toggle(server, master, lathe);
            Toggle(server, master, otherMaster);
            Toggle(server, dept, master);
            Toggle(server, dept, otherDept);
            Assert.Multiple(() =>
            {
                Assert.That(masterComp.Clients, Is.Empty);
                Assert.That(deptComp.Clients, Is.Empty);
                Assert.That(entMan.GetComponent<OreSiloClientComponent>(lathe).Silo, Is.Null);
                Assert.That(entMan.GetComponent<OreSiloClientComponent>(master).Silo, Is.Null);
            });

            Assert.That(storage.TryChangeMaterialAmount(dept, Steel, 100));
            Assert.That(storage.TryChangeMaterialAmount(lathe, Steel, 40));
            Toggle(server, master, dept);
            Toggle(server, otherMaster, dept);
            Toggle(server, dept, lathe);
            Toggle(server, otherDept, lathe);
            Assert.Multiple(() =>
            {
                Assert.That(storage.GetMaterialAmount(lathe, Steel, localOnly: true), Is.EqualTo(0));
                Assert.That(storage.GetMaterialAmount(dept, Steel, localOnly: true), Is.EqualTo(140));
                Assert.That(storage.GetMaterialAmount(master, Steel, localOnly: true), Is.EqualTo(0));
                Assert.That(masterComp.Clients, Is.EquivalentTo(new[] { dept }));
                Assert.That(otherMasterComp.Clients, Is.Empty);
                Assert.That(entMan.GetComponent<OreSiloClientComponent>(dept).Silo, Is.EqualTo(master));
                Assert.That(deptComp.Clients, Is.EquivalentTo(new[] { lathe }));
                Assert.That(otherDeptComp.Clients, Is.Empty);
                Assert.That(entMan.GetComponent<OreSiloClientComponent>(lathe).Silo, Is.EqualTo(dept));
            });

            Toggle(server, otherMaster, otherDept);
            Assert.That(otherMasterComp.Clients, Is.EquivalentTo(new[] { otherDept }));
            entMan.DeleteEntity(otherDept);
            Assert.That(otherMasterComp.Clients, Is.Empty);
            entMan.DeleteEntity(master);
            Assert.That(entMan.GetComponent<OreSiloClientComponent>(dept).Silo, Is.Null);
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task MasterDistributesToDepartments()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var map = await pair.CreateTestMap();

        var entMan = server.EntMan;
        var storage = server.System<SharedMaterialStorageSystem>();
        var silos = server.System<OreSiloSystem>();

        await server.WaitAssertion(() =>
        {
            var master = Spawn(server, map, Master);
            var dept = Spawn(server, map, Department);
            var otherDept = Spawn(server, map, Department);
            var lathe = Spawn(server, map, Lathe);

            var masterEnt = (master, entMan.GetComponent<OreSiloComponent>(master));
            var deptEnt = (dept, entMan.GetComponent<OreSiloComponent>(dept));

            int Local(EntityUid uid) => storage.GetMaterialAmount(uid, Steel, localOnly: true);
            int Total(EntityUid uid) => storage.GetMaterialAmount(uid, Steel);
            bool Send(EntityUid target, int sheets) => silos.TryTransferMaterial(masterEnt, target, Steel, sheets, out _);

            void AssertStock(int masterStock, int deptStock, int latheStock, string step)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(Local(master), Is.EqualTo(masterStock), $"{step}: master stock");
                    Assert.That(Local(dept), Is.EqualTo(deptStock), $"{step}: department stock");
                    Assert.That(Local(lathe), Is.EqualTo(latheStock), $"{step}: lathe stock");
                    Assert.That(Total(dept), Is.EqualTo(deptStock), $"{step}: visible to department silo");
                    Assert.That(Total(lathe), Is.EqualTo(deptStock + latheStock), $"{step}: visible to lathe");
                });
            }

            Assert.That(storage.TryChangeMaterialAmount(master, Steel, 1000));
            Assert.That(storage.TryChangeMaterialAmount(dept, Steel, 500));

            Assert.That(Send(dept, 1), Is.False);

            Toggle(server, master, dept);
            Toggle(server, dept, lathe);
            AssertStock(1000, 500, 0, "linked");

            Assert.That(storage.TryChangeMaterialAmount(lathe, Steel, -300));
            AssertStock(1000, 200, 0, "department stock used");
            Assert.That(storage.TryChangeMaterialAmount(lathe, Steel, -300), Is.False);
            AssertStock(1000, 200, 0, "overdraw");

            Assert.That(storage.TryChangeMaterialAmount(lathe, Steel, 100));
            AssertStock(1000, 300, 0, "insert");

            Assert.That(Send(dept, 3));
            AssertStock(700, 600, 0, "sent 3 sheets");

            Assert.Multiple(() =>
            {
                Assert.That(Send(dept, 0), Is.False, "zero sheets");
                Assert.That(Send(dept, -5), Is.False, "negative sheets");
                Assert.That(Send(otherDept, 1), Is.False, "unlinked silo");
                Assert.That(Send(lathe, 1), Is.False, "lathe");
                Assert.That(silos.TryTransferMaterial(masterEnt, dept, "NotAMaterial", 1, out _), Is.False, "bad material");
                Assert.That(silos.TryTransferMaterial(deptEnt, lathe, Steel, 1, out _), Is.False, "department silo as source");
            });
            AssertStock(700, 600, 0, "invalid requests");

            SetPowered(server, master, false);
            Assert.That(Send(dept, 1), Is.False);
            SetPowered(server, master, true);
            AssertStock(700, 600, 0, "master unpowered");

            Assert.That(silos.TryTransferMaterial(masterEnt, dept, Steel, 50, out var sent));
            Assert.That(sent, Is.EqualTo(700));
            AssertStock(0, 1300, 0, "sent the rest");
            Assert.That(Send(dept, 1), Is.False);

            SetPowered(server, dept, false);
            Assert.That(storage.TryChangeMaterialAmount(lathe, Steel, 50));
            SetPowered(server, dept, true);
            AssertStock(0, 1300, 50, "lathe has leftovers");
            Assert.That(storage.TryChangeMaterialAmount(lathe, Steel, -1320));
            AssertStock(0, 0, 30, "split");

            Assert.That(storage.TryChangeMaterialAmount(master, Steel, 200));
            Assert.That(storage.TryChangeMaterialAmount(dept, Steel, 400));
            Toggle(server, master, dept);
            AssertStock(200, 400, 30, "unlinked");
        });

        await pair.CleanReturnAsync();
    }

    private static EntityUid Spawn(RobustIntegrationTest.ServerIntegrationInstance server, TestMapData map, string proto)
    {
        var uid = server.EntMan.SpawnEntity(proto, map.GridCoords);
        server.System<SharedPowerReceiverSystem>().SetNeedsPower(uid, false);
        SetPowered(server, uid, true);
        return uid;
    }

    private static void SetPowered(RobustIntegrationTest.ServerIntegrationInstance server, EntityUid uid, bool powered)
    {
        server.EntMan.GetComponent<ApcPowerReceiverComponent>(uid).Powered = powered;
    }

    private static void Toggle(RobustIntegrationTest.ServerIntegrationInstance server, EntityUid silo, EntityUid client)
    {
        var msg = new ToggleOreSiloClientMessage(server.EntMan.GetNetEntity(client));
        server.EntMan.EventBus.RaiseLocalEvent(silo, msg);
    }
}
