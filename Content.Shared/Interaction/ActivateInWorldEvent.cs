// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using JetBrains.Annotations;

namespace Content.Shared.Interaction;

/// <summary>
///     Raised when an entity is activated in the world.
/// </summary>
[PublicAPI]
public sealed class ActivateInWorldEvent : HandledEntityEventArgs, ITargetedInteractEventArgs
{
    /// <summary>
    ///     Entity that activated the target world entity.
    /// </summary>
    public EntityUid User { get; }

    /// <summary>
    ///     Entity that was activated in the world.
    /// </summary>
    public EntityUid Target { get; }

    /// <summary>
    ///     Whether or not <see cref="User"/> can perform complex interactions or only basic ones.
    /// </summary>
    public bool Complex;

    /// <summary>
    ///     Set to true when the activation is logged by a specific logger.
    /// </summary>
    public bool WasLogged { get; set; }

    public ActivateInWorldEvent(EntityUid user, EntityUid target, bool complex)
    {
        User = user;
        Target = target;
        Complex = complex;
    }
}

/// <summary>
/// Event raised on the user when it activates something in the world
/// </summary>
[PublicAPI]
public sealed class UserActivateInWorldEvent : HandledEntityEventArgs, ITargetedInteractEventArgs
{
    /// <summary>
    ///     Entity that activated the target world entity.
    /// </summary>
    public EntityUid User { get; }

    /// <summary>
    ///     Entity that was activated in the world.
    /// </summary>
    public EntityUid Target { get; }

    /// <summary>
    ///     Whether or not <see cref="User"/> can perform complex interactions or only basic ones.
    /// </summary>
    public bool Complex;

    public UserActivateInWorldEvent(EntityUid user, EntityUid target, bool complex)
    {
        User = user;
        Target = target;
        Complex = complex;
    }
}