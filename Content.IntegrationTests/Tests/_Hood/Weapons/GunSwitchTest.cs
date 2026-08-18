// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Hood.Weapons;
using Content.Shared._Hood.Clothing;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.GameObjects;

namespace Content.IntegrationTests.Tests._Hood.Weapons;

[TestFixture]
public sealed class GunSwitchTest
{
    [Test]
    public async Task GroundedLongGunsUseNativeAmmoAndFireModeSystems()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var testMap = await pair.CreateTestMap();
        var entities = server.ResolveDependency<IEntityManager>();
        var itemSlots = entities.System<ItemSlotsSystem>();

        await server.WaitAssertion(() =>
        {
            var rook = entities.SpawnEntity("HoodWeaponCarbineRookC9", testMap.GridCoords);
            var arroyo = entities.SpawnEntity("HoodWeaponRifleArroyoR12", testMap.GridCoords);
            var mesa = entities.SpawnEntity("HoodWeaponShotgunMesaP12", testMap.GridCoords);

            var rookGun = entities.GetComponent<GunComponent>(rook);
            var arroyoGun = entities.GetComponent<GunComponent>(arroyo);
            var mesaGun = entities.GetComponent<GunComponent>(mesa);
            Assert.Multiple(() =>
            {
                Assert.That(rookGun.AvailableModes.HasFlag(SelectiveFire.SemiAuto), Is.True);
                Assert.That(rookGun.AvailableModes.HasFlag(SelectiveFire.FullAuto), Is.True);
                Assert.That(arroyoGun.AvailableModes, Is.EqualTo(SelectiveFire.SemiAuto));
                Assert.That(mesaGun.AvailableModes, Is.EqualTo(SelectiveFire.SemiAuto));
                Assert.That(itemSlots.TryGetSlot(rook, SharedGunSystem.MagazineSlot, out var rookMagazine), Is.True);
                Assert.That(rookMagazine!.HasItem, Is.True);
                Assert.That(itemSlots.TryGetSlot(arroyo, SharedGunSystem.MagazineSlot, out var rifleMagazine), Is.True);
                Assert.That(rifleMagazine!.HasItem, Is.True);
                Assert.That(entities.HasComponent<BallisticAmmoProviderComponent>(mesa), Is.True);
            });
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task SwitchUsesNativeFireModesAndRestoresFallback()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var testMap = await pair.CreateTestMap();
        var entities = server.ResolveDependency<IEntityManager>();
        var itemSlots = entities.System<ItemSlotsSystem>();

        await server.WaitAssertion(() =>
        {
            var models = new[]
            {
                "HoodWeaponPistolGlorpG1",
                "HoodWeaponPistolGlorpC2",
                "HoodWeaponPistolGlorpS3",
                "HoodWeaponPistolGlorpL4",
            };

            var gun = EntityUid.Invalid;
            foreach (var model in models)
            {
                var spawned = entities.SpawnEntity(model, testMap.GridCoords);
                gun = gun.IsValid() ? gun : spawned;

                var modelGun = entities.GetComponent<GunComponent>(spawned);
                Assert.Multiple(() =>
                {
                    Assert.That(modelGun.AvailableModes, Is.EqualTo(SelectiveFire.SemiAuto), model);
                    Assert.That(entities.HasComponent<GunSwitchCompatibleComponent>(spawned), Is.True, model);
                    Assert.That(entities.HasComponent<SuppressEquippedVisualInBeltComponent>(spawned), Is.True, model);
                    Assert.That(itemSlots.TryGetSlot(spawned, SharedGunSystem.MagazineSlot, out var magazine), Is.True, model);
                    Assert.That(magazine!.HasItem, Is.True, model);
                });
            }

            var gunSwitch = entities.SpawnEntity("HoodGunSwitch", testMap.GridCoords);
            var duplicate = entities.SpawnEntity("HoodGunSwitch", testMap.GridCoords);
            var incompatibleGun = entities.SpawnEntity("WeaponPistolMk58", testMap.GridCoords);
            var component = entities.GetComponent<GunComponent>(gun);

            Assert.Multiple(() =>
            {
                Assert.That(component.AvailableModes, Is.EqualTo(SelectiveFire.SemiAuto));
                Assert.That(itemSlots.TryInsert(incompatibleGun,
                    GunSwitchCompatibleComponent.SwitchSlotId,
                    gunSwitch,
                    null), Is.False);
                Assert.That(itemSlots.TryInsert(gun,
                    GunSwitchCompatibleComponent.SwitchSlotId,
                    gunSwitch,
                    null), Is.True);
                Assert.That(component.AvailableModes.HasFlag(SelectiveFire.FullAuto), Is.True);
                Assert.That(itemSlots.TryInsert(gun,
                    GunSwitchCompatibleComponent.SwitchSlotId,
                    duplicate,
                    null), Is.False);
            });

            component.SelectedMode = SelectiveFire.FullAuto;

            Assert.That(itemSlots.TryEject(gun,
                GunSwitchCompatibleComponent.SwitchSlotId,
                null,
                out var ejected,
                doAfter: false), Is.True);

            Assert.Multiple(() =>
            {
                Assert.That(ejected, Is.EqualTo(gunSwitch));
                Assert.That(component.AvailableModes, Is.EqualTo(SelectiveFire.SemiAuto));
                Assert.That(component.SelectedMode, Is.EqualTo(SelectiveFire.SemiAuto));
            });
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task DeletingAttachedSwitchRestoresValidState()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var testMap = await pair.CreateTestMap();
        var entities = server.ResolveDependency<IEntityManager>();
        var itemSlots = entities.System<ItemSlotsSystem>();
        EntityUid gun = default;
        EntityUid gunSwitch = default;

        await server.WaitAssertion(() =>
        {
            gun = entities.SpawnEntity("HoodWeaponPistolGlorpC2", testMap.GridCoords);
            gunSwitch = entities.SpawnEntity("HoodGunSwitch", testMap.GridCoords);
            Assert.That(itemSlots.TryInsert(gun,
                GunSwitchCompatibleComponent.SwitchSlotId,
                gunSwitch,
                null), Is.True);

            var component = entities.GetComponent<GunComponent>(gun);
            component.SelectedMode = SelectiveFire.Invalid;
            entities.QueueDeleteEntity(gunSwitch);
        });

        await server.WaitRunTicks(1);

        await server.WaitAssertion(() =>
        {
            var component = entities.GetComponent<GunComponent>(gun);
            Assert.Multiple(() =>
            {
                Assert.That(component.AvailableModes, Is.EqualTo(SelectiveFire.SemiAuto));
                Assert.That(component.SelectedMode, Is.EqualTo(SelectiveFire.SemiAuto));
            });
        });

        await pair.CleanReturnAsync();
    }
}
