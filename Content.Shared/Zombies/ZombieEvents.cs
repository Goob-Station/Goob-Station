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


/// <summary>
///  Goobstation
///  Event raised when unzombifying or something.
///  Does nothing on client server handles unzombifying after an entity effect.
/// </summary>
[ByRefEvent]
public readonly struct EntityUnZombifiedEvent
{
    /// <summary>
    ///     Whether this person should be inoculated from catching the infection again.
    /// </summary>
    public readonly bool Inoculate;

    public EntityUnZombifiedEvent(bool inoculate)
    {
        Inoculate = inoculate;
    }
};
