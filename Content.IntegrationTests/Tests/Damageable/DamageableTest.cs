// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2021 ColdAutumnRain <73938872+ColdAutumnRain@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Galactic Chimp <GalacticChimpanzee@gmail.com>
// SPDX-FileCopyrightText: 2021 Jaskanbe <86671825+Jaskanbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Kara Dinyes <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2021 Leon Friedrich <60421075+leonsfriedrich@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Leon Friedrich <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2021 Michael Will <will_m@outlook.de>
// SPDX-FileCopyrightText: 2021 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 SETh lafuente <cetaciocascarudo@gmail.com>
// SPDX-FileCopyrightText: 2021 ScalyChimp <72841710+scaly-chimp@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 SethLafuente <84478872+SethLafuente@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Silver <Silvertorch5@gmail.com>
// SPDX-FileCopyrightText: 2021 Silver <silvertorch5@gmail.com>
// SPDX-FileCopyrightText: 2021 Swept <sweptwastaken@protonmail.com>
// SPDX-FileCopyrightText: 2021 TimrodDX <timrod@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 mirrorcult <notzombiedude@gmail.com>
// SPDX-FileCopyrightText: 2021 scrato <Mickaello2003@gmx.de>
// SPDX-FileCopyrightText: 2022 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 KrasnoshchekovPavel <119816022+KrasnoshchekovPavel@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests.Damageable
{
    [TestFixture]
    [TestOf(typeof(DamageableComponent))]
    [TestOf(typeof(DamageableSystem))]
    public sealed class DamageableTest
    {
        [TestPrototypes]
        private const string Prototypes = @"
# Define some damage groups
- type: damageType
  id: TestDamage1
  name: damage-type-blunt

- type: damageType
  id: TestDamage2a
  name: damage-type-blunt

- type: damageType
  id: TestDamage2b
  name: damage-type-blunt

- type: damageType
  id: TestDamage3a
  name: damage-type-blunt

- type: damageType
  id: TestDamage3b
  name: damage-type-blunt

- type: damageType
  id: TestDamage3c
  name: damage-type-blunt

# Define damage Groups with 1,2,3 damage types
- type: damageGroup
  id: TestGroup1
  name: damage-group-brute
  damageTypes:
    - TestDamage1

- type: damageGroup
  id: TestGroup2
  name: damage-group-brute
  damageTypes:
    - TestDamage2a
    - TestDamage2b

- type: damageGroup
  id: TestGroup3
  name: damage-group-brute
  damageTypes:
    - TestDamage3a
    - TestDamage3b
    - TestDamage3c

# This container should not support TestDamage1 or TestDamage2b
- type: damageContainer
  id: testDamageContainer
  supportedGroups:
    - TestGroup3
  supportedTypes:
    - TestDamage2a

- type: entity
  id: TestDamageableEntityId
  name: TestDamageableEntityId
  components:
  - type: Damageable
    damageContainer: testDamageContainer
";

        [Test]
        public async Task TestDamageableComponents()
        {
            await using var pair = await PoolManager.GetServerClient();
            var server = pair.Server;

            var sEntityManager = server.ResolveDependency<IEntityManager>();
            var sMapManager = server.ResolveDependency<IMapManager>();
            var sPrototypeManager = server.ResolveDependency<IPrototypeManager>();
            var sEntitySystemManager = server.ResolveDependency<IEntitySystemManager>();

            EntityUid sDamageableEntity = default;
            DamageableComponent sDamageableComponent = null;
            DamageableSystem sDamageableSystem = null;

            DamageGroupPrototype group1 = default!;
            DamageGroupPrototype group2 = default!;
            DamageGroupPrototype group3 = default!;

            DamageTypePrototype type1 = default!;
            DamageTypePrototype type2a = default!;
            DamageTypePrototype type2b = default!;
            DamageTypePrototype type3a = default!;
            DamageTypePrototype type3b = default!;
            DamageTypePrototype type3c = default!;

            FixedPoint2 typeDamage;

            var map = await pair.CreateTestMap();

            await server.WaitPost(() =>
            {
                var coordinates = map.MapCoords;

                sDamageableEntity = sEntityManager.SpawnEntity("TestDamageableEntityId", coordinates);
                sDamageableComponent = sEntityManager.GetComponent<DamageableComponent>(sDamageableEntity);
                sDamageableSystem = sEntitySystemManager.GetEntitySystem<DamageableSystem>();

                group1 = sPrototypeManager.Index<DamageGroupPrototype>("TestGroup1");
                group2 = sPrototypeManager.Index<DamageGroupPrototype>("TestGroup2");
                group3 = sPrototypeManager.Index<DamageGroupPrototype>("TestGroup3");

                type1 = sPrototypeManager.Index<DamageTypePrototype>("TestDamage1");
                type2a = sPrototypeManager.Index<DamageTypePrototype>("TestDamage2a");
                type2b = sPrototypeManager.Index<DamageTypePrototype>("TestDamage2b");
                type3a = sPrototypeManager.Index<DamageTypePrototype>("TestDamage3a");
                type3b = sPrototypeManager.Index<DamageTypePrototype>("TestDamage3b");
                type3c = sPrototypeManager.Index<DamageTypePrototype>("TestDamage3c");
            });

            await server.WaitRunTicks(5);

            await server.WaitAssertion(() =>
            {
                var uid = sDamageableEntity;

                // Check that the correct types are supported.
                Assert.Multiple(() =>
                {
                    Assert.That(sDamageableComponent.Damage.DamageDict.ContainsKey(type1.ID), Is.False);
                    Assert.That(sDamageableComponent.Damage.DamageDict.ContainsKey(type2a.ID), Is.True);
                    Assert.That(sDamageableComponent.Damage.DamageDict.ContainsKey(type2b.ID), Is.False);
                    Assert.That(sDamageableComponent.Damage.DamageDict.ContainsKey(type3a.ID), Is.True);
                    Assert.That(sDamageableComponent.Damage.DamageDict.ContainsKey(type3b.ID), Is.True);
                    Assert.That(sDamageableComponent.Damage.DamageDict.ContainsKey(type3c.ID), Is.True);
                });

                // Check that damage is evenly distributed over a group if its a nice multiple
                var types = group3.DamageTypes;
                var damageToDeal = FixedPoint2.New(types.Count * 5);
                DamageSpecifier damage = new(group3, damageToDeal);

                sDamageableSystem.TryChangeDamage(uid, damage, true);

                Assert.Multiple(() =>
                {
                    Assert.That(sDamageableComponent.TotalDamage, Is.EqualTo(damageToDeal));
                    Assert.That(sDamageableComponent.DamagePerGroup[group3.ID], Is.EqualTo(damageToDeal));
                    foreach (var type in types)
                    {
                        Assert.That(sDamageableComponent.Damage.DamageDict.TryGetValue(type, out typeDamage));
                        Assert.That(typeDamage, Is.EqualTo(damageToDeal / types.Count));
                    }
                });

                // Heal
                sDamageableSystem.TryChangeDamage(uid, -damage);

                Assert.Multiple(() =>
                {
                    Assert.That(sDamageableComponent.TotalDamage, Is.EqualTo(FixedPoint2.Zero));
                    Assert.That(sDamageableComponent.DamagePerGroup[group3.ID], Is.EqualTo(FixedPoint2.Zero));
                    foreach (var type in types)
                    {
                        Assert.That(sDamageableComponent.Damage.DamageDict.TryGetValue(type, out typeDamage));
                        Assert.That(typeDamage, Is.EqualTo(FixedPoint2.Zero));
                    }
                });

                // Check that damage works properly if it is NOT perfectly divisible among group members
                types = group3.DamageTypes;

                Assert.That(types, Has.Count.EqualTo(3));

                damage = new DamageSpecifier(group3, 14);
                sDamageableSystem.TryChangeDamage(uid, damage, true);

                Assert.Multiple(() =>
                {
                    Assert.That(sDamageableComponent.TotalDamage, Is.EqualTo(FixedPoint2.New(14)));
                    Assert.That(sDamageableComponent.DamagePerGroup[group3.ID], Is.EqualTo(FixedPoint2.New(14)));
                    Assert.That(sDamageableComponent.Damage.DamageDict[type3a.ID], Is.EqualTo(FixedPoint2.New(4.66f)));
                    Assert.That(sDamageableComponent.Damage.DamageDict[type3b.ID], Is.EqualTo(FixedPoint2.New(4.67f)));
                    Assert.That(sDamageableComponent.Damage.DamageDict[type3c.ID], Is.EqualTo(FixedPoint2.New(4.67f)));
                });

                // Heal
                sDamageableSystem.TryChangeDamage(uid, -damage);

                Assert.Multiple(() =>
                {
                    Assert.That(sDamageableComponent.TotalDamage, Is.EqualTo(FixedPoint2.Zero));
                    Assert.That(sDamageableComponent.DamagePerGroup[group3.ID], Is.EqualTo(FixedPoint2.Zero));
                    foreach (var type in types)
                    {
                        Assert.That(sDamageableComponent.Damage.DamageDict.TryGetValue(type, out typeDamage));
                        Assert.That(typeDamage, Is.EqualTo(FixedPoint2.Zero));
                    }

                    // Test that unsupported groups return false when setting/getting damage (and don't change damage)
                    Assert.That(sDamageableComponent.TotalDamage, Is.EqualTo(FixedPoint2.Zero));
                });
                damage = new DamageSpecifier(group1, FixedPoint2.New(10)) + new DamageSpecifier(type2b, FixedPoint2.New(10));
                sDamageableSystem.TryChangeDamage(uid, damage, true);

                Assert.Multiple(() =>
                {
                    Assert.That(sDamageableComponent.DamagePerGroup.TryGetValue(group1.ID, out _), Is.False);
                    Assert.That(sDamageableComponent.Damage.DamageDict.TryGetValue(type1.ID, out typeDamage), Is.False);
                    Assert.That(sDamageableComponent.TotalDamage, Is.EqualTo(FixedPoint2.Zero));
                });

                // Test SetAll function
                sDamageableSystem.SetAllDamage(sDamageableEntity, sDamageableComponent, 10);
                Assert.That(sDamageableComponent.TotalDamage, Is.EqualTo(FixedPoint2.New(10 * sDamageableComponent.Damage.DamageDict.Count)));
                sDamageableSystem.SetAllDamage(sDamageableEntity, sDamageableComponent, 0);
                Assert.That(sDamageableComponent.TotalDamage, Is.EqualTo(FixedPoint2.Zero));

                // Test 'wasted' healing
                sDamageableSystem.TryChangeDamage(uid, new DamageSpecifier(type3a, 5));
                sDamageableSystem.TryChangeDamage(uid, new DamageSpecifier(type3b, 7));
                sDamageableSystem.TryChangeDamage(uid, new DamageSpecifier(group3, -11));

                Assert.Multiple(() =>
                {
                    Assert.That(sDamageableComponent.Damage.DamageDict[type3a.ID], Is.EqualTo(FixedPoint2.New(1.34)));
                    Assert.That(sDamageableComponent.Damage.DamageDict[type3b.ID], Is.EqualTo(FixedPoint2.New(3.33)));
                    Assert.That(sDamageableComponent.Damage.DamageDict[type3c.ID], Is.EqualTo(FixedPoint2.New(0)));
                });

                // Test Over-Healing
                sDamageableSystem.TryChangeDamage(uid, new DamageSpecifier(group3, FixedPoint2.New(-100)));
                Assert.That(sDamageableComponent.TotalDamage, Is.EqualTo(FixedPoint2.Zero));

                // Test that if no health change occurred, returns false
                sDamageableSystem.TryChangeDamage(uid, new DamageSpecifier(group3, -100));
                Assert.That(sDamageableComponent.TotalDamage, Is.EqualTo(FixedPoint2.Zero));
            });
            await pair.CleanReturnAsync();
        }
    }
}