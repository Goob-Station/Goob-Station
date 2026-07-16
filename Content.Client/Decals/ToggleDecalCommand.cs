// SPDX-License-Identifier: AGPL-3.0-or-later

﻿using Robust.Shared.Console;

namespace Content.Client.Decals;

public sealed class ToggleDecalCommand : LocalizedEntityCommands
{
    [Dependency] private readonly DecalSystem _decal = default!;

    public override string Command => "toggledecals";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        _decal.ToggleOverlay();
    }
}