// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.IoC;
using Content.Client.Parallax.Managers;
using Robust.Client;
using Robust.Shared.Configuration;
using Robust.Shared.ContentPack;
using Robust.Shared.IoC;
using Robust.Shared.Log;
using Robust.UnitTesting;

namespace Content.IntegrationTests.Tests.Utility;

public sealed class SandboxTest
{
    [Test]
    public async Task Test()
    {
        // Not using PoolManager.GetServerClient() because we want to avoid having to unnecessarily create & destroy a
        // server. This all becomes unnecessary if ever the test becomes non-destructive or the no-server option
        // actually creates a pair without a server.

        // To hell with creating a pair. Sandbox is client only  -Misandrie

        var asm = PoolManager.GetAssemblies(true, false);

        var logHandler = new PoolTestLogHandler("CLIENT");
        logHandler.ActivateContext(TestContext.Out);
        var options = new RobustIntegrationTest.ClientIntegrationOptions
        {
            ContentStart = true,
            OverrideLogHandler = () => logHandler,
            ContentAssemblies = asm,
            Options = new GameControllerOptions { LoadConfigAndUserData = false }
        };

        options.BeforeStart += () =>
        {
            IoCManager.Resolve<IModLoader>().SetModuleBaseCallbacks(new ClientModuleTestingCallbacks
            {
                ClientBeforeIoC = () =>
                {
                    IoCManager.Register<IParallaxManager, DummyParallaxManager>(true);
                    IoCManager.Resolve<ILogManager>().GetSawmill("loc").Level = LogLevel.Error;
                    IoCManager.Resolve<IConfigurationManager>()
                        .OnValueChanged(RTCVars.FailureLogLevel, value => logHandler.FailureLevel = value, true);
                }
            });
        };

        using var client = new RobustIntegrationTest.ClientIntegrationInstance(options);
        await client.WaitIdleAsync();

        foreach (var assembly in asm)
        {
            await client.CheckSandboxed(assembly);
        }
    }
}