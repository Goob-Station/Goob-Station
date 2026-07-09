// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameObjects;

namespace Content.Goobstation.Common.Interactions;

/// <summary>
///     UseAttempt, but for item.
/// </summary>
public sealed class UseInHandAttemptEvent(EntityUid user) : CancellableEntityEventArgs
{
    public EntityUid User { get; } = user;
}