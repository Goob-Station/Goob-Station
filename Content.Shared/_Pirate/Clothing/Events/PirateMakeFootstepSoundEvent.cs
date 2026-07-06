// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Inventory;
using Robust.Shared.Audio;

namespace Content.Shared._Pirate.Clothing.Events;

/// <summary>
/// Relays each generated footstep to worn gear that wants its own movement sound.
/// </summary>
[ByRefEvent]
public record struct PirateMakeFootstepSoundEvent : IInventoryRelayEvent
{
    public SlotFlags TargetSlots => SlotFlags.WITHOUT_POCKET;

    /// <summary>
    /// Optional replacement for the normal footstep sound.
    /// </summary>
    public SoundSpecifier? OverrideSound;
}
