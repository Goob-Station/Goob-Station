// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.IntegrationTests.Tests.Interaction;
using Content.Server.Power.Components;
using Content.Shared.Materials;
using Content.Shared.Materials.OreSilo;
using Content.Shared.Power.EntitySystems;

namespace Content.IntegrationTests.Tests._Goobstation.Materials;

public sealed class MasterSiloAccessTest : InteractionTest
{
    [Test]
    public async Task InsertWithoutAccess()
    {
        await SpawnTarget("MachineMaterialSiloMaster");

        await Server.WaitPost(() =>
        {
            var silo = SEntMan.GetEntity(Target!.Value);
            SEntMan.System<SharedPowerReceiverSystem>().SetNeedsPower(silo, false);
            SEntMan.GetComponent<ApcPowerReceiverComponent>(silo).Powered = true;
        });
        await RunTicks(5);

        await InteractUsing("Steel", 5);
        await RunTicks(5);

        await Server.WaitAssertion(() =>
        {
            var silo = SEntMan.GetEntity(Target!.Value);
            var storage = SEntMan.System<SharedMaterialStorageSystem>();
            Assert.That(storage.GetMaterialAmount(silo, "Steel", localOnly: true), Is.EqualTo(500));
        });

        await Activate();
        await RunTicks(5);
        Assert.That(IsUiOpen(OreSiloUiKey.Key), Is.False);
    }
}
