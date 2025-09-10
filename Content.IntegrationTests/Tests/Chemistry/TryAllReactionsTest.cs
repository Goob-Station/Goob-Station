// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Javier Guardia Fernández <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2022 Timothy Teakettle <59849408+timothyteakettle@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2023 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Threading.Tasks;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Chemistry.Reaction;
using NUnit.Framework;
using Robust.Shared.Prototypes;
using Robust.UnitTesting;

namespace Content.IntegrationTests.Tests.Chemistry
{
    [TestFixture]
    public sealed class TryAllReactionsTest : RobustIntegrationTest
    {
        [Test]
        public async Task TryAllTest()
        {
            var server = StartServer();
            await server.WaitIdleAsync();

            var protoManager = server.ResolveDependency<IPrototypeManager>();
            var reactions = protoManager.EnumeratePrototypes<ReactionPrototype>().ToList();

            await server.WaitAssertion(() =>
            {
                foreach (var reaction in reactions)
                {
                    // Skip disabled or special-case reactions if needed
                    if (reaction.Reactants.Count == 0 || reaction.Products.Count == 0)
                        continue;

                    var foundProductsMap = reaction.Products;

                    foreach (var (reagent, quantity) in foundProductsMap)
                    {
                        var success = foundProductsMap.TryFirstOrNull(
                            x => x.Key.Key == reagent && x.Key.Value == quantity,
                            out var foundProduct
                        );

                        Assert.That(
                            success,
                            $"[FAILED] Reaction '{reaction.ID}' did not produce expected reagent '{reagent}' with quantity {quantity}. " +
                            $"Available outputs: [{string.Join(", ", foundProductsMap.Select(p => $"{p.Key.Key}:{p.Key.Value}"))}]"
                        );
                    }
                }
            });
        }
    }
}
