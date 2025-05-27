// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 yglop <95057024+yglop@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.SpawnOnInteract;

[RegisterComponent]
public sealed partial class SpawnOnInteractComponent : Component
{
    [DataField]
    public string ToSpawn = "";

    [DataField]
    public bool DeletInteractedEnt = true;

    [DataField]
    public bool DeletInteractingEnt = true;

    [DataField]
    public bool GibUser = true;
}