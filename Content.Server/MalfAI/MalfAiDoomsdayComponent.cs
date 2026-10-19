// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Server.MalfAI;

/// <summary>
/// Server-only component tracking an active Doomsday Protocol triggered by a Malf AI.
/// Attached to the AI entity while active.
/// </summary>
[RegisterComponent]
public sealed partial class MalfAiDoomsdayComponent : Component
{
    /// <summary>
    /// Whether the doomsday protocol is currently active.
    /// </summary>
    [ViewVariables]
    public bool Active;

    /// <summary>
    /// Owning station for which the alert level was changed.
    /// </summary>
    [ViewVariables]
    public EntityUid Station;

    /// <summary>
    /// Previously active alert level on the station before switching to cyan.
    /// </summary>
    [ViewVariables]
    public string PrevAlertLevel = string.Empty;

    /// <summary>
    /// Whether the station alert was locked before activation.
    /// </summary>
    [ViewVariables]
    public bool PrevLocked;

    /// <summary>
    /// Duration of the selected doomsday song in seconds.
    /// </summary>
    [ViewVariables]
    public float? SongDuration;

    /// <summary>
    /// Whether the doomsday music has been started.
    /// </summary>
    [ViewVariables]
    public bool MusicStarted;

    /// <summary>
    /// The selected doomsday song for this instance.
    /// </summary>
    [ViewVariables]
    public ResolvedSoundSpecifier? SelectedDoomsdaySong;

    /// <summary>
    /// Remaining countdown time.
    /// </summary>
    [ViewVariables]
    public TimeSpan RemainingTime;

    /// <summary>
    /// The AI core holder entity recorded at activation time.
    /// If the AI leaves this holder, doomsday is aborted.
    /// </summary>
    [ViewVariables]
    public EntityUid CoreHolder;
}
