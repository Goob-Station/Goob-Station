// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Audio;

namespace Content.Shared.Movement.Events;

/// <summary>
/// Raised directed on an entity when trying to get a relevant footstep sound
/// </summary>
[ByRefEvent]
public record struct GetFootstepSoundEvent(EntityUid User)
{
    public readonly EntityUid User = User;

    /// <summary>
    /// Set the sound to specify a footstep sound and mark as handled.
    /// </summary>
    public SoundSpecifier? Sound;
}