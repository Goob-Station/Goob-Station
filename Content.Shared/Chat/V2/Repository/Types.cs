// SPDX-FileCopyrightText: 2024 Hannah Giovanna Dawson <karakkaraz@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Your Name <you@example.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Runtime.InteropServices;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared.Chat.V2.Repository;

/// <summary>
/// The record associated with a specific chat event.
/// </summary>
public struct ChatRecord(string userName, NetUserId userId, IChatEvent storedEvent, string entityName)
{
    public string UserName = userName;
    public NetUserId UserId = userId;
    public string EntityName = entityName;
    public IChatEvent StoredEvent = storedEvent;
}

/// <summary>
/// Notifies that a chat message has been created.
/// </summary>
/// <param name="ev"></param>
[Serializable, NetSerializable]
public sealed class MessageCreatedEvent(IChatEvent ev) : EntityEventArgs
{
    public IChatEvent Event = ev;
}

/// <summary>
/// Notifies that a chat message has been changed.
/// </summary>
/// <param name="id"></param>
/// <param name="newMessage"></param>
[Serializable, NetSerializable]
public sealed class MessagePatchedEvent(uint id, string newMessage) : EntityEventArgs
{
    public uint MessageId = id;
    public string NewMessage = newMessage;
}

/// <summary>
/// Notifies that a chat message has been deleted.
/// </summary>
/// <param name="id"></param>
[Serializable, NetSerializable]
public sealed class MessageDeletedEvent(uint id) : EntityEventArgs
{
    public uint MessageId = id;
}

/// <summary>
/// Notifies that a player's messages have been nuked.
/// </summary>
/// <param name="set"></param>
[Serializable, NetSerializable]
public sealed class MessagesNukedEvent(List<uint> set) : EntityEventArgs
{
    public uint[] MessageIds = CollectionsMarshal.AsSpan(set).ToArray();
}
