// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 Wrexbe (Josh) <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 wrexbe <wrexbe@protonmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Console;

namespace Content.Client.Access.Commands;

public sealed class ShowAccessReadersCommand : IConsoleCommand
{
    public string Command => "showaccessreaders";

    public string Description => "Toggles showing access reader permissions on the map";
    public string Help => """
        Overlay Info:
        -Disabled | The access reader is disabled
        +Unrestricted | The access reader has no restrictions
        +Set [Index]: [Tag Name]| A tag in an access set (accessor needs all tags in the set to be allowed by the set)
        +Key [StationUid]: [StationRecordKeyId] | A StationRecordKey that is allowed
        -Tag [Tag Name] | A tag that is not allowed (takes priority over other allows)
        """;
    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var collection = IoCManager.Instance;

        if (collection == null)
            return;

        var overlay = collection.Resolve<IOverlayManager>();

        if (overlay.RemoveOverlay<AccessOverlay>())
        {
            shell.WriteLine($"Set access reader debug overlay to false");
            return;
        }

        var entManager = collection.Resolve<IEntityManager>();
        var cache = collection.Resolve<IResourceCache>();
        var xform = entManager.System<SharedTransformSystem>();

        overlay.AddOverlay(new AccessOverlay(entManager, cache, xform));
        shell.WriteLine($"Set access reader debug overlay to true");
    }
}