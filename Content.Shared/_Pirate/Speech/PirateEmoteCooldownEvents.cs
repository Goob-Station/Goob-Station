// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Pirate.Speech;

[ByRefEvent]
public sealed class PirateEmoteCooldownAttemptEvent(EntityUid source) : CancellableEntityEventArgs
{
    public EntityUid Source = source;
}

[ByRefEvent]
public sealed class PirateEmoteCooldownCommitEvent(EntityUid source) : EntityEventArgs
{
    public EntityUid Source = source;
}
