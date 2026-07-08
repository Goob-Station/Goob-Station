// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Weapons;

/// <summary>
///     Allows this melee weapon to relay the damage and take it from some other sources, for example gun attachments.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MeleeWeaponRelayComponent : Component;

[ByRefEvent]
public record struct GetRelayMeleeWeaponEvent(EntityUid? Found = null, bool Handled = false);
