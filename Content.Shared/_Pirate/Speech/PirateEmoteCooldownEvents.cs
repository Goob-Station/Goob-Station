// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Pirate.Speech;

[ByRefEvent]
public sealed class PirateEmoteCooldownAttemptEvent : CancellableEntityEventArgs
{
}

[ByRefEvent]
public sealed class PirateEmoteCooldownCommitEvent : EntityEventArgs
{
}
