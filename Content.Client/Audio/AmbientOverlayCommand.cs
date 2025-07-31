// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Kyle Tyo <36606155+VerinSenpai@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Console;

namespace Content.Client.Audio;

public sealed class AmbientOverlayCommand : LocalizedEntityCommands
{
    [Dependency] private readonly AmbientSoundSystem _ambient = default!;

    public override string Command => "showambient";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        _ambient.OverlayEnabled ^= true;

        shell.WriteLine(Loc.GetString($"cmd-showambient-status", ("status", _ambient.OverlayEnabled)));
    }
}