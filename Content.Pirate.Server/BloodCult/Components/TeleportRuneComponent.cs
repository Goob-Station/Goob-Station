// SPDX-FileCopyrightText: 2024 Remuchi <72476615+Remuchi@users.noreply.github.com>
// SPDX-FileCopyrightText: 2026 v0id <>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;

namespace Content.Server.BloodCult.Components;

[RegisterComponent]
public sealed partial class TeleportRuneComponent : Component
{
    [DataField]
    public float GatherRange = 0.65f;

    [DataField]
    public string Name = string.Empty;

    [DataField]
    public SoundSpecifier TeleportInSound = new SoundPathSpecifier("/Audio/_Pirate/BloodCult/veilin.ogg");

    [DataField]
    public SoundSpecifier TeleportOutSound = new SoundPathSpecifier("/Audio/_Pirate/BloodCult/veilout.ogg");
}
