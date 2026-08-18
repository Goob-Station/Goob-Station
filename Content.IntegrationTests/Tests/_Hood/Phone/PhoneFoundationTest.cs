// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Generic;
using Content.Shared._Hood.Phone;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Inventory;
using Content.Shared.Storage;
using Robust.Shared.GameObjects;

namespace Content.IntegrationTests.Tests._Hood.Phone;

[TestFixture]
public sealed class PhoneFoundationTest
{
    [TestPrototypes]
    private const string TestPrototypes = @"
- type: entity
  id: HoodPhoneInventoryTestMob
  components:
  - type: Inventory
  - type: ContainerContainer

- type: entity
  id: HoodPhoneInventoryTestImpostor
  components:
  - type: Item
    size: Small
  - type: Clothing
    slots:
    - PHONE
";

    [Test]
    public async Task DedicatedInventorySlotAcceptsOnlyPhones()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var testMap = await pair.CreateTestMap();
        var entities = server.ResolveDependency<IEntityManager>();
        var inventory = entities.System<InventorySystem>();

        await server.WaitAssertion(() =>
        {
            var mob = entities.SpawnEntity("HoodPhoneInventoryTestMob", testMap.GridCoords);
            var impostor = entities.SpawnEntity("HoodPhoneInventoryTestImpostor", testMap.GridCoords);

            Assert.Multiple(() =>
            {
                Assert.That(inventory.HasSlot(mob, "phone"), Is.True);
                Assert.That(inventory.HasSlot(mob, "id"), Is.True);
                Assert.That(inventory.HasSlot(mob, "belt"), Is.True);
                Assert.That(inventory.HasSlot(mob, "pocket1"), Is.True);
                Assert.That(inventory.HasSlot(mob, "pocket2"), Is.True);
                Assert.That(inventory.HasSlot(mob, "back"), Is.True);
                Assert.That(inventory.CanEquip(mob, impostor, "phone", out _), Is.False);
            });

            foreach (var prototype in new[] { "HoodPhoneStreetline", "HoodPhoneSunset", "HoodPhonePacific" })
            {
                var phone = entities.SpawnEntity(prototype, testMap.GridCoords);
                Assert.That(inventory.CanEquip(mob, phone, "phone", out _), Is.True, prototype);
                Assert.That(inventory.TryEquip(mob, phone, "phone"), Is.True, prototype);
                Assert.That(inventory.TryUnequip(mob, "phone"), Is.True, prototype);
            }

            foreach (var (boxPrototype, phonePrototype) in new[]
                     {
                         ("HoodPhoneStreetlineBox", "HoodPhoneStreetline"),
                         ("HoodPhoneSunsetBox", "HoodPhoneSunset"),
                         ("HoodPhonePacificBox", "HoodPhonePacific"),
                     })
            {
                var box = entities.SpawnEntity(boxPrototype, testMap.GridCoords);
                var storage = entities.GetComponent<StorageComponent>(box);
                var contents = new HashSet<string>();

                foreach (var contained in storage.Container.ContainedEntities)
                {
                    var prototypeId = entities.GetComponent<MetaDataComponent>(contained).EntityPrototype?.ID;
                    Assert.That(prototypeId, Is.Not.Null, boxPrototype);
                    contents.Add(prototypeId!);
                }

                Assert.That(
                    contents,
                    Is.EquivalentTo(new[] { phonePrototype, "HoodSimTool" }),
                    boxPrototype);
            }
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task SimNumbersAndSlotLifecycleAreAuthoritative()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var testMap = await pair.CreateTestMap();
        var entities = server.ResolveDependency<IEntityManager>();
        var itemSlots = entities.System<ItemSlotsSystem>();

        await server.WaitAssertion(() =>
        {
            var phone = entities.SpawnEntity("HoodPhoneStreetline", testMap.GridCoords);
            var firstSim = entities.SpawnEntity("HoodSimCard", testMap.GridCoords);
            var secondSim = entities.SpawnEntity("HoodSimCard", testMap.GridCoords);
            var unrelated = entities.SpawnEntity("HoodPhoneInventoryTestImpostor", testMap.GridCoords);

            var firstNumber = entities.GetComponent<SimCardComponent>(firstSim).Number;
            var secondNumber = entities.GetComponent<SimCardComponent>(secondSim).Number;

            Assert.Multiple(() =>
            {
                Assert.That(firstNumber, Is.InRange(1000u, 9999u));
                Assert.That(secondNumber, Is.InRange(1000u, 9999u));
                Assert.That(secondNumber, Is.Not.EqualTo(firstNumber));
                Assert.That(itemSlots.TryInsert(phone, PhoneComponent.SimSlotId, unrelated, null), Is.False);
                Assert.That(itemSlots.TryInsert(phone, PhoneComponent.SimSlotId, firstSim, null), Is.True);
            });

            var phoneComponent = entities.GetComponent<PhoneComponent>(phone);
            Assert.That(phoneComponent.SimSlot.Item, Is.EqualTo(firstSim));

            Assert.That(itemSlots.TryEject(
                phone,
                PhoneComponent.SimSlotId,
                null,
                out var ejected,
                doAfter: false), Is.True);
            Assert.That(ejected, Is.EqualTo(firstSim));
            Assert.That(phoneComponent.SimSlot.Item, Is.Null);
        });

        await pair.CleanReturnAsync();
    }
}
