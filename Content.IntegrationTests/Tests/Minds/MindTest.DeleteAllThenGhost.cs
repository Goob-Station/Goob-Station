// SPDX-FileCopyrightText: 2024 Firewatch <54725557+musicmanvr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mr. 27 <45323883+Dutch-VanDerLinde@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mr. 27 <koolthunder019@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

#nullable enable
using Robust.Shared.Console;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;

namespace Content.IntegrationTests.Tests.Minds;

[TestFixture]
public sealed partial class MindTests
{
    [Test]
    public async Task DeleteAllThenGhost()
    {
        var settings = new PoolSettings
        {
            Dirty = true,
            DummyTicker = false,
            Connected = true
        };
        await using var pair = await PoolManager.GetServerClient(settings);

        // Client is connected with a valid entity & mind
        Assert.That(pair.Client.EntMan.EntityExists(pair.Client.AttachedEntity));
        Assert.That(pair.Server.EntMan.EntityExists(pair.PlayerData?.Mind));

        // Delete **everything**
        var conHost = pair.Server.ResolveDependency<IConsoleHost>();
        await pair.Server.WaitPost(() => conHost.ExecuteCommand("entities delete"));
        await pair.RunTicksSync(5);

        Assert.That(pair.Server.EntMan.EntityCount, Is.EqualTo(0));

        foreach (var ent in pair.Client.EntMan.GetEntities())
        {
            Console.WriteLine(pair.Client.EntMan.ToPrettyString(ent));
        }

        Assert.That(pair.Client.EntMan.EntityCount, Is.EqualTo(0));

        // Create a new map.
        MapId mapId = default;
        await pair.Server.WaitPost(() => pair.Server.System<SharedMapSystem>().CreateMap(out mapId));
        await pair.RunTicksSync(5);

        // Client is not attached to anything
        Assert.That(pair.Client.AttachedEntity, Is.Null);
        Assert.That(pair.PlayerData?.Mind, Is.Null);

        // Attempt to ghost
        var cConHost = pair.Client.ResolveDependency<IConsoleHost>();
        await pair.Client.WaitPost(() => cConHost.ExecuteCommand("ghost"));
        await pair.RunTicksSync(10);

        // Client should be attached to a ghost placed on the new map.
        Assert.That(pair.Client.EntMan.EntityExists(pair.Client.AttachedEntity));
        Assert.That(pair.Server.EntMan.EntityExists(pair.PlayerData?.Mind));
        var xform = pair.Client.Transform(pair.Client.AttachedEntity!.Value);
        Assert.That(xform.MapID, Is.EqualTo(mapId));

        await pair.CleanReturnAsync();
    }
}