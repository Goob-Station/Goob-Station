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
using Content.Shared.Atmos.Prototypes;

namespace Content.IntegrationTests.Tests.Atmos;

[TestOf(typeof(Atmospherics))]
public sealed class ConstantsTest
{
    [Test]
    public async Task TotalGasesTest()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var entityManager = server.EntMan;
        var protoManager = server.ProtoMan;

        await server.WaitPost(() =>
        {
            var atmosSystem = entityManager.System<AtmosphereSystem>();

            Assert.Multiple(() =>
            {
                // adding new gases needs a few changes in the code, so make sure this is done everywhere
                var gasProtos = protoManager.EnumeratePrototypes<GasPrototype>().ToList();

                // number of gas prototypes
                Assert.That(gasProtos, Has.Count.EqualTo(Atmospherics.TotalNumberOfGases),
                     $"Number of GasPrototypes is not equal to TotalNumberOfGases.");
                // number of gas prototypes used in the atmos system
                Assert.That(atmosSystem.Gases.Count(), Is.EqualTo(Atmospherics.TotalNumberOfGases),
                     $"AtmosSystem.Gases is not equal to TotalNumberOfGases.");
                // enum mapping gases to their Id
                Assert.That(Enum.GetValues<Gas>(), Has.Length.EqualTo(Atmospherics.TotalNumberOfGases),
                     $"Gas enum size is not equal to TotalNumberOfGases.");
                // localized abbreviations for UI purposes
                Assert.That(Atmospherics.GasAbbreviations, Has.Count.EqualTo(Atmospherics.TotalNumberOfGases),
                     $"GasAbbreviations size is not equal to TotalNumberOfGases.");

                // the ID for each gas has to correspond to a value in the Gas enum (converted to a string)
                foreach (var gas in gasProtos)
                {
                    Assert.That(Enum.TryParse<Gas>(gas.ID, out _), $"GasPrototype {gas.ID} has an invalid ID. It must correspond to a value in the {nameof(Gas)} enum.");
                }
            });
        });
        await pair.CleanReturnAsync();
    }
}
