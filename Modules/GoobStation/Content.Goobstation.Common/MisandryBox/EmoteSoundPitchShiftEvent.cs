// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.MisandryBox;

/// <summary>
/// Used by <see cref="CatEmoteSpamCountermeasureSystem"/> to pitch emote sounds as it nears to a smite
/// </summary>
/// <param name="pitch">additive pitch to the sound</param>
[ByRefEvent]
public struct EmoteSoundPitchShiftEvent(float pitch = 0)
{
    public float Pitch { get; set; } = pitch;
}
