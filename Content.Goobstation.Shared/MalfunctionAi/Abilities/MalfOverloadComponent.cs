// SPDX-FileCopyrightText: 2026 Jonikibaka <153797633+Jonikibaka@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameObjects;

namespace Content.Goobstation.Shared.MalfunctionAi;

/// <summary>
/// Tuning for the Overload Machine ability: detonates a targeted powered machine.
/// </summary>
[RegisterComponent]
public sealed partial class MalfOverloadComponent : Component
{
    /// <summary>Total intensity of the overload explosion.</summary>
    [DataField] public float Intensity = 60f;

    /// <summary>Per-tile intensity cap of the overload explosion.</summary>
    [DataField] public float MaxTileIntensity = 10f;

    /// <summary>Falloff slope of the overload explosion. Lower values make a wider, more even blast.</summary>
    [DataField] public float Slope = 2f;

    /// <summary>Delay between triggering an overload and the explosion, giving a warning window.</summary>
    [DataField] public TimeSpan Delay = TimeSpan.FromSeconds(5);

    /// <summary>Warning sound played at the targeted machine when an overload starts.</summary>
    [DataField] public SoundSpecifier WarningSound = new SoundPathSpecifier("/Audio/Machines/vessel_warning.ogg");
}
