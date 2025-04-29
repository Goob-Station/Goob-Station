// SPDX-FileCopyrightText: 2024 Jezithyr <jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Hands.Components;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Prototypes;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests.Puller;

#nullable enable

[TestFixture]
public sealed class PullerTest
{
    /// <summary>
    /// Checks that needsHands on PullerComponent is not set on mobs that don't even have hands.
    /// </summary>
    [Test]
    public async Task PullerSanityTest()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;

        var compFactory = server.ResolveDependency<IComponentFactory>();
        var protoManager = server.ResolveDependency<IPrototypeManager>();

        await server.WaitAssertion(() =>
        {
            Assert.Multiple(() =>
            {
                foreach (var proto in protoManager.EnumeratePrototypes<EntityPrototype>())
                {
                    if (!proto.TryGetComponent(out PullerComponent? puller, compFactory))
                        continue;

                    if (!puller.NeedsHands)
                        continue;

                    Assert.That(proto.HasComponent<HandsComponent>(compFactory), $"Found puller {proto} with NeedsHand pulling but has no hands?");
                }
            });
        });

        await pair.CleanReturnAsync();
    }
}