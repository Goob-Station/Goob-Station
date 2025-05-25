// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Holopad;

/// <summary>
/// Holds data pertaining to entities that are using holopads
/// </summary>
/// <remarks>
/// This component is added and removed automatically from entities
/// </remarks>
[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedHolopadSystem))]
public sealed partial class HolopadUserComponent : Component
{
    /// <summary>
    /// A list of holopads that the user is interacting with
    /// </summary>
    [ViewVariables]
    public HashSet<Entity<HolopadComponent>> LinkedHolopads = new();
}

/// <summary>
/// A networked event raised when the visual state of a hologram is being updated
/// </summary>
[Serializable, NetSerializable]
public sealed class HolopadUserTypingChangedEvent : EntityEventArgs
{
    /// <summary>
    /// The hologram being updated
    /// </summary>
    public readonly NetEntity User;

    /// <summary>
    /// The typing indicator state
    /// </summary>
    public readonly bool IsTyping;

    public HolopadUserTypingChangedEvent(NetEntity user, bool isTyping)
    {
        User = user;
        IsTyping = isTyping;
    }
}