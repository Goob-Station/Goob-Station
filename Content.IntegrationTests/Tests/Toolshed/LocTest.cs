// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Moony <moony@hellomouse.net>
// SPDX-FileCopyrightText: 2023 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Robust.Shared.Localization;
using Robust.Shared.Toolshed;

namespace Content.IntegrationTests.Tests.Toolshed;

// this is an EXACT DUPLICATE of LocTest from robust. If you modify this, modify that too.
// Anyone who fails to heed these instructions consents to being scrungled to death.
[TestFixture]
public sealed class LocTest : ToolshedTest
{
    [Test]
    public async Task AllCommandsHaveDescriptions()
    {
        var locMan = Server.ResolveDependency<ILocalizationManager>();
        var toolMan = Server.ResolveDependency<ToolshedManager>();
        var locStrings = new HashSet<string>();

        var ignored = new HashSet<Assembly>()
            {typeof(LocTest).Assembly, typeof(Robust.UnitTesting.Shared.Toolshed.LocTest).Assembly};

        await Server.WaitAssertion(() =>
        {
            Assert.Multiple(() =>
            {
                foreach (var cmd in toolMan.DefaultEnvironment.AllCommands())
                {
                    if (ignored.Contains(cmd.Cmd.GetType().Assembly))
                        continue;

                    var descLoc = cmd.DescLocStr();
                    Assert.That(locStrings.Add(descLoc), $"Duplicate command description key: {descLoc}");
                    Assert.That(locMan.TryGetString(descLoc, out _), $"Failed to get command description for command {cmd.FullName()}");
                }
            });
        });
    }
}