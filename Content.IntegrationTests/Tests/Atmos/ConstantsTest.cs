// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Javier Guardia Fernández <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System.Linq;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Robust.Shared.GameObjects;

namespace Content.IntegrationTests.Tests.Atmos
{
    [TestFixture]
    [TestOf(typeof(Atmospherics))]
    public sealed class ConstantsTest
    {
        [Test]
        public async Task TotalGasesTest()
        {
            await using var pair = await PoolManager.GetServerClient();
            var server = pair.Server;
            var entityManager = server.ResolveDependency<IEntityManager>();

            await server.WaitPost(() =>
            {
                var atmosSystem = entityManager.System<AtmosphereSystem>();

                Assert.Multiple(() =>
                {
                    Assert.That(atmosSystem.Gases.Count(), Is.EqualTo(Atmospherics.TotalNumberOfGases));
                    Assert.That(Enum.GetValues(typeof(Gas)), Has.Length.EqualTo(Atmospherics.TotalNumberOfGases));
                });
            });
            await pair.CleanReturnAsync();
        }
    }
}