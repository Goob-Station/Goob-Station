// SPDX-FileCopyrightText: 2020 DamianX <DamianX@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2021 ColdAutumnRain <73938872+ColdAutumnRain@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Galactic Chimp <GalacticChimpanzee@gmail.com>
// SPDX-FileCopyrightText: 2021 Jaskanbe <86671825+Jaskanbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Javier Guardia Fernández <DrSmugleaf@users.noreply.github.com>
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
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <wrexbe@protonmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <drsmugleaf@gmail.com>
// SPDX-FileCopyrightText: 2023 Jezithyr <jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Moony <moony@hellomouse.net>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 faint <46868845+ficcialfaint@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration.Commands;
using Content.Server.Administration.Systems;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests.Commands
{
    [TestFixture]
    [TestOf(typeof(RejuvenateSystem))]
    public sealed class RejuvenateTest
    {
        [TestPrototypes]
        private const string Prototypes = @"
- type: entity
  name: DamageableDummy
  id: DamageableDummy
  components:
  - type: Damageable
    damageContainer: Biological
  - type: MobState
  - type: MobThresholds
    thresholds:
      0: Alive
      200: Dead
";

        [Test]
        public async Task RejuvenateDeadTest()
        {
            await using var pair = await PoolManager.GetServerClient();
            var server = pair.Server;
            var entManager = server.ResolveDependency<IEntityManager>();
            var prototypeManager = server.ResolveDependency<IPrototypeManager>();
            var mobStateSystem = entManager.System<MobStateSystem>();
            var damSystem = entManager.System<DamageableSystem>();
            var rejuvenateSystem = entManager.System<RejuvenateSystem>();

            await server.WaitAssertion(() =>
            {
                var human = entManager.SpawnEntity("DamageableDummy", MapCoordinates.Nullspace);
                DamageableComponent damageable = null;
                MobStateComponent mobState = null;

                // Sanity check
                Assert.Multiple(() =>
                {
                    Assert.That(entManager.TryGetComponent(human, out damageable));
                    Assert.That(entManager.TryGetComponent(human, out mobState));
                });
                Assert.Multiple(() =>
                {
                    Assert.That(mobStateSystem.IsAlive(human, mobState), Is.True);
                    Assert.That(mobStateSystem.IsCritical(human, mobState), Is.False);
                    Assert.That(mobStateSystem.IsDead(human, mobState), Is.False);
                    Assert.That(mobStateSystem.IsIncapacitated(human, mobState), Is.False);
                });

                // Kill the entity
                DamageSpecifier damage = new(prototypeManager.Index<DamageGroupPrototype>("Toxin"), FixedPoint2.New(10000000));

                damSystem.TryChangeDamage(human, damage, true);

                // Check that it is dead
                Assert.Multiple(() =>
                {
                    Assert.That(mobStateSystem.IsAlive(human, mobState), Is.False);
                    Assert.That(mobStateSystem.IsCritical(human, mobState), Is.False);
                    Assert.That(mobStateSystem.IsDead(human, mobState), Is.True);
                    Assert.That(mobStateSystem.IsIncapacitated(human, mobState), Is.True);
                });

                // Rejuvenate them
                rejuvenateSystem.PerformRejuvenate(human);

                // Check that it is alive and with no damage
                Assert.Multiple(() =>
                {
                    Assert.That(mobStateSystem.IsAlive(human, mobState), Is.True);
                    Assert.That(mobStateSystem.IsCritical(human, mobState), Is.False);
                    Assert.That(mobStateSystem.IsDead(human, mobState), Is.False);
                    Assert.That(mobStateSystem.IsIncapacitated(human, mobState), Is.False);

                    Assert.That(damageable.TotalDamage, Is.EqualTo(FixedPoint2.Zero));
                });
            });
            await pair.CleanReturnAsync();
        }
    }
}