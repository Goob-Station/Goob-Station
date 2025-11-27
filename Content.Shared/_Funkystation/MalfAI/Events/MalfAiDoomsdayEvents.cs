// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared._Funkystation.MalfAI.Events;

/// <summary>
/// Raised when the Malf AI Doomsday Protocol countdown starts.
/// </summary>
public sealed class MalfAiDoomsdayStartedEvent : EntityEventArgs
{
    public EntityUid Station { get; }
    public EntityUid Ai { get; }

    public MalfAiDoomsdayStartedEvent(EntityUid station, EntityUid ai)
    {
        Station = station;
        Ai = ai;
    }
}

/// <summary>
/// Raised when the Malf AI Doomsday Protocol countdown completes (was not aborted).
/// </summary>
public sealed class MalfAiDoomsdayCompletedEvent : EntityEventArgs
{
    public EntityUid Station { get; }
    public EntityUid Ai { get; }

    public MalfAiDoomsdayCompletedEvent(EntityUid station, EntityUid ai)
    {
        Station = station;
        Ai = ai;
    }
}
