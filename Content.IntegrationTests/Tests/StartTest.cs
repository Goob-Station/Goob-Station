// SPDX-FileCopyrightText: 2019 ZelteHonor <gabrieldionbouchard@gmail.com>
// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 Javier Guardia Fernández <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Exceptions;

namespace Content.IntegrationTests.Tests
{
    [TestFixture]
    public sealed class StartTest
    {
        /// <summary>
        ///     Test that the server, and client start, and stop.
        /// </summary>
        [Test]
        public async Task TestClientStart()
        {
            await using var pair = await PoolManager.GetServerClient();
            var client = pair.Client;
            Assert.That(client.IsAlive);
            await client.WaitRunTicks(5);
            Assert.That(client.IsAlive);
            var cRuntimeLog = client.ResolveDependency<IRuntimeLog>();
            Assert.That(cRuntimeLog.ExceptionCount, Is.EqualTo(0), "No exceptions must be logged on client.");
            await client.WaitIdleAsync();
            Assert.That(client.IsAlive);

            var server = pair.Server;
            Assert.That(server.IsAlive);
            var sRuntimeLog = server.ResolveDependency<IRuntimeLog>();
            Assert.That(sRuntimeLog.ExceptionCount, Is.EqualTo(0), "No exceptions must be logged on server.");
            await server.WaitIdleAsync();
            Assert.That(server.IsAlive);

            await pair.CleanReturnAsync();
        }
    }
}