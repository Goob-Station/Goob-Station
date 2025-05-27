// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 yglop <95057024+yglop@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.SpawnOnInteract;

[RegisterComponent]
public sealed partial class SpawnOnInteractComponent : Component
{
    [DataField]
    public string ToSpawn = string.Empty;

    [DataField]
    public bool DeleteInteractedEnt = true;

    [DataField]
    public bool DeleteInteractingEnt = true;

    [DataField]
    public bool GibUser = true;
}