// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
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