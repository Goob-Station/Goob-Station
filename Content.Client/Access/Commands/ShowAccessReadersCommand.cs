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

public sealed class ShowAccessReadersCommand : LocalizedEntityCommands
{
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly IResourceCache _cache = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;

    public override string Command => "showaccessreaders";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var existing = _overlay.RemoveOverlay<AccessOverlay>();
        if (!existing)
            _overlay.AddOverlay(new AccessOverlay(EntityManager, _cache, _xform));

        shell.WriteLine(Loc.GetString($"cmd-showaccessreaders-status", ("status", !existing)));
    }
}