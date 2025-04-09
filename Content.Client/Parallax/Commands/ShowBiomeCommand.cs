// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Client.Graphics;
using Robust.Shared.Console;

namespace Content.Client.Parallax.Commands;

public sealed class ShowBiomeCommand : LocalizedCommands
{
    [Dependency] private readonly IOverlayManager _overlayMgr = default!;

    public override string Command => "showbiome";
    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (_overlayMgr.HasOverlay<BiomeDebugOverlay>())
        {
            _overlayMgr.RemoveOverlay<BiomeDebugOverlay>();
        }
        else
        {
            _overlayMgr.AddOverlay(new BiomeDebugOverlay());
        }
    }
}