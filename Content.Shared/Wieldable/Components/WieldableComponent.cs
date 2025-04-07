// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 WarMechanic <69510347+WarMechanic@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 stopbreaking <126102320+stopbreaking@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Wieldable.Components;

/// <summary>
///     Used for objects that can be wielded in two or more hands,
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedWieldableSystem)), AutoGenerateComponentState]
public sealed partial class WieldableComponent : Component
{
    [DataField("wieldSound")]
    public SoundSpecifier? WieldSound = new SoundPathSpecifier("/Audio/Effects/thudswoosh.ogg");

    [DataField("unwieldSound")]
    public SoundSpecifier? UnwieldSound;

    /// <summary>
    ///     Number of free hands required (excluding the item itself) required
    ///     to wield it
    /// </summary>
    [DataField("freeHandsRequired")]
    public int FreeHandsRequired = 1;

    [AutoNetworkedField, DataField("wielded")]
    public bool Wielded = false;

    /// <summary>
    ///     Whether using the item inhand while wielding causes the item to unwield.
    ///     Unwielding can conflict with other inhand actions.
    /// </summary>
    [DataField]
    public bool UnwieldOnUse = true;

    /// <summary>
    ///     Should use delay trigger after the wield/unwield?
    /// </summary>
    [DataField]
    public bool UseDelayOnWield = true;

    [DataField("wieldedInhandPrefix")]
    public string? WieldedInhandPrefix = "wielded";

    public string? OldInhandPrefix = null;
}

[Serializable, NetSerializable]
public enum WieldableVisuals : byte
{
    Wielded
}