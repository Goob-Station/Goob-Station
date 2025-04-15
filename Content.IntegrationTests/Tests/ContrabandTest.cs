// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ignaz "Ian" Kraft <ignaz.k@live.de>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Contraband;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed class ContrabandTest
{
    [Test]
    public async Task EntityShowDepartmentsAndJobs()
    {
        await using var pair = await PoolManager.GetServerClient();
        var client = pair.Client;
        var protoMan = client.ResolveDependency<IPrototypeManager>();
        var componentFactory = client.ResolveDependency<IComponentFactory>();

        await client.WaitAssertion(() =>
        {
            Assert.Multiple(() =>
            {
                foreach (var proto in protoMan.EnumeratePrototypes<EntityPrototype>())
                {
                    if (proto.Abstract || pair.IsTestPrototype(proto))
                        continue;

                    if (!proto.TryGetComponent<ContrabandComponent>(out var contraband, componentFactory))
                        continue;

                    Assert.That(protoMan.TryIndex(contraband.Severity, out var severity, false),
                        @$"{proto.ID} has a ContrabandComponent with a unknown severity.");

                    if (!severity.ShowDepartmentsAndJobs)
                        continue;

                    Assert.That(contraband.AllowedDepartments.Count + contraband.AllowedJobs.Count, Is.Not.EqualTo(0),
                        @$"{proto.ID} has a ContrabandComponent with ShowDepartmentsAndJobs but no allowed departments or jobs.");
                }
            });
        });

        await pair.CleanReturnAsync();
    }
}