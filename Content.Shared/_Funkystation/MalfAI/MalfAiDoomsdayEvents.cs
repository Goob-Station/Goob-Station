// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

namespace Content.Shared._Funkystation.MalfAI;

/// <summary>
/// Raised broadcast when a Malf AI initiates doomsday.
/// </summary>
public sealed class MalfAiDoomsdayStartedEvent : EntityEventArgs
{
    public EntityUid Station;
    public EntityUid Ai;

    public MalfAiDoomsdayStartedEvent(EntityUid station, EntityUid ai)
    {
        Station = station;
        Ai = ai;
    }
}

/// <summary>
/// Raised broadcast when the doomsday completes.
/// </summary>
public sealed class MalfAiDoomsdayCompletedEvent : EntityEventArgs
{
    public EntityUid Station;
    public EntityUid Ai;

    public MalfAiDoomsdayCompletedEvent(EntityUid station, EntityUid ai)
    {
        Station = station;
        Ai = ai;
    }
}
