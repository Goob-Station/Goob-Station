// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Actions;

namespace Content.Shared.Zombies;

/// <summary>
///     Event that is broadcast whenever an entity is zombified.
///     Used by the zombie gamemode to track total infections.
/// </summary>
[ByRefEvent]
public readonly struct EntityZombifiedEvent
{
    /// <summary>
    ///     The entity that was zombified.
    /// </summary>
    public readonly EntityUid Target;

    public EntityZombifiedEvent(EntityUid target)
    {
        Target = target;
    }
};

/// <summary>
///     Event raised when a player zombifies themself using the "turn" action
/// </summary>
public sealed partial class ZombifySelfActionEvent : InstantActionEvent { };