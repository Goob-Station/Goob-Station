// SPDX-FileCopyrightText: 2026 Jonikibaka <153797633+Jonikibaka@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameObjects;

namespace Content.Goobstation.Shared.MalfunctionAi;

/// <summary>
/// Tuning for the Detonate RCDs ability: rigs every RCD on the grid to explode.
/// </summary>
[RegisterComponent]
public sealed partial class MalfRcdComponent : Component
{
    /// <summary>Delay between rigging RCDs and their explosion, giving holders a warning window.</summary>
    [DataField] public TimeSpan Delay = TimeSpan.FromSeconds(5);

    /// <summary>Intensity for the RCD detonation explosion. Kept small: this is a utility, not a bomb.</summary>
    [DataField] public float Intensity = 10f;

    /// <summary>Per-tile intensity cap for the RCD detonation explosion.</summary>
    [DataField] public float MaxTileIntensity = 4f;

    /// <summary>Falloff slope of the RCD detonation explosion.</summary>
    [DataField] public float Slope = 3f;

    /// <summary>Warning beep played at each rigged RCD.</summary>
    [DataField] public SoundSpecifier WarningSound = new SoundPathSpecifier("/Audio/Machines/Nuke/general_beep.ogg");
}
