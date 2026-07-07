// SPDX-FileCopyrightText: 2026 Jonikibaka <153797633+Jonikibaka@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameObjects;

namespace Content.Goobstation.Shared.MalfunctionAi;

/// <summary>
/// Tuning for the Hack Cyborg ability.
/// </summary>
[RegisterComponent]
public sealed partial class MalfHackCyborgComponent : Component
{
    /// <summary>
    /// Played to a cyborg player when the AI subverts it — the same malfunction theme
    /// the AI hears on becoming the antagonist.
    /// </summary>
    [DataField] public SoundSpecifier SubvertSound = new SoundPathSpecifier("/Audio/_Goobstation/Ambience/Antag/malf.ogg");
}
