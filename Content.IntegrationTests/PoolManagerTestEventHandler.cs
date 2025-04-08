// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SX-7 <92227810+SX-7@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SX_7 <sn1.test.preria.2002@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.IntegrationTests;

[SetUpFixture]
public sealed class PoolManagerTestEventHandler
{
    // This value is completely arbitrary.
    private static TimeSpan MaximumTotalTestingTimeLimit => TimeSpan.FromMinutes(20);
    private static TimeSpan HardStopTimeLimit => MaximumTotalTestingTimeLimit.Add(TimeSpan.FromMinutes(1));

    [OneTimeSetUp]
    public void Setup()
    {
        PoolManager.Startup();
        // If the tests seem to be stuck, we try to end it semi-nicely
        _ = Task.Delay(MaximumTotalTestingTimeLimit).ContinueWith(_ =>
        {
            // This can and probably will cause server/client pairs to shut down MID test, and will lead to really confusing test failures.
            TestContext.Error.WriteLine($"\n\n{nameof(PoolManagerTestEventHandler)}: ERROR: Tests are taking too long. Shutting down all tests. This may lead to weird failures/exceptions.\n\n");
            PoolManager.Shutdown();
        });

        // If ending it nicely doesn't work within a minute, we do something a bit meaner.
        _ = Task.Delay(HardStopTimeLimit).ContinueWith(_ =>
        {
            var deathReport = PoolManager.DeathReport();
            Environment.FailFast($"Tests took way too ;\n Death Report:\n{deathReport}");
        });
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        PoolManager.Shutdown();
    }
}