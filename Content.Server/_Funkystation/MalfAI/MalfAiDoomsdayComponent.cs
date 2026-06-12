// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

namespace Content.Server._Funkystation.MalfAI;

/// <summary>
/// Added to a Malf AI when Doomsday countdown is active.
/// </summary>
[RegisterComponent]
public sealed partial class MalfAiDoomsdayComponent : Component
{
    [DataField]
    public bool Active;

    [DataField]
    public EntityUid? Station;

    [DataField]
    public string? PrevAlertLevel;

    [DataField]
    public bool PrevLocked;

    [DataField]
    public float SongDuration;

    [DataField]
    public bool MusicStarted;

    [DataField]
    public string? SelectedDoomsdaySong;

    [DataField]
    public TimeSpan RemainingTime;

    [DataField]
    public EntityUid? CoreHolder;
}
