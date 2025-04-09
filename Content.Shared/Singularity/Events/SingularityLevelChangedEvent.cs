// SPDX-FileCopyrightText: 2022 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Singularity.Components;

namespace Content.Shared.Singularity.Events;

/// <summary>
/// An event raised whenever a singularity changes its level.
/// </summary>
public sealed class SingularityLevelChangedEvent : EntityEventArgs
{
    /// <summary>
    /// The new level of the singularity.
    /// </summary>
    public readonly byte NewValue;

    /// <summary>
    /// The previous level of the singularity.
    /// </summary>
    public readonly byte OldValue;

    /// <summary>
    /// The singularity that just changed level.
    /// </summary>
    public readonly SingularityComponent Singularity;

    public SingularityLevelChangedEvent(byte newValue, byte oldValue, SingularityComponent singularity)
    {
        NewValue = newValue;
        OldValue = oldValue;
        Singularity = singularity;
    }
}