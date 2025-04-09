// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
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

using Robust.Shared.GameObjects;

namespace Content.IntegrationTests.Tests.Networking
{
    [TestFixture]
    public sealed class NetworkIdsMatchTest
    {
        [Test]
        public async Task TestConnect()
        {
            await using var pair = await PoolManager.GetServerClient(new PoolSettings { Connected = true });
            var server = pair.Server;
            var client = pair.Client;

            var clientCompFactory = client.ResolveDependency<IComponentFactory>();
            var serverCompFactory = server.ResolveDependency<IComponentFactory>();

            var clientNetComps = clientCompFactory.NetworkedComponents;
            var serverNetComps = serverCompFactory.NetworkedComponents;

            Assert.Multiple(() =>
            {
                Assert.That(clientNetComps, Is.Not.Null);
                Assert.That(serverNetComps, Is.Not.Null);
            });
            Assert.Multiple(() =>
            {
                Assert.That(clientNetComps, Has.Count.EqualTo(serverNetComps.Count));

                // Checks that at least Metadata and Transform are registered.
                Assert.That(clientNetComps, Has.Count.GreaterThanOrEqualTo(2));
            });

            Assert.Multiple(() =>
            {
                for (var netId = 0; netId < clientNetComps.Count; netId++)
                {
                    Assert.That(clientNetComps[netId].Name, Is.EqualTo(serverNetComps[netId].Name));
                }
            });
            await pair.CleanReturnAsync();
        }
    }
}