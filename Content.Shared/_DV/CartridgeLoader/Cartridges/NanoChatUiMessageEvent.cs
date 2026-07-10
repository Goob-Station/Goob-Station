// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.CartridgeLoader;
using Robust.Shared.Serialization;
using System.Linq; // Pirate: camera (nanochat gallery)

namespace Content.Shared._DV.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed class NanoChatUiMessageEvent : CartridgeMessageEvent
{
    /// <summary>
    ///     The type of UI message being sent.
    /// </summary>
    public readonly NanoChatUiMessageType Type;

    /// <summary>
    ///     The recipient's NanoChat number, if applicable.
    /// </summary>
    public readonly uint? RecipientNumber;

    /// <summary>
    ///     The content of the message or name for new chats.
    /// </summary>
    public readonly string? Content;

    /// <summary>
    ///     The recipient's job title when creating a new chat.
    /// </summary>
    public readonly string? RecipientJob;

    #region Pirate: camera (nanochat gallery)
    /// <summary>
    ///     The gallery file name associated with this action, if applicable.
    /// </summary>
    public readonly string? PhotoFileName;

    /// <summary>
    ///     The message id associated with this action, if applicable.
    /// </summary>
    public readonly uint? MessageId;
    #endregion

    /// <summary>
    ///     Creates a new NanoChat UI message event.
    /// </summary>
    /// <param name="type">The type of message being sent</param>
    /// <param name="recipientNumber">Optional recipient number for the message</param>
    /// <param name="content">Optional content of the message</param>
    /// <param name="recipientJob">Optional job title for new chat creation</param>
    /// <param name="photoFileName">Optional gallery file name for attachments and deletes</param> // Pirate: camera (nanochat gallery)
    /// <param name="messageId">Optional message id for message photo actions</param>// Pirate: camera (nanochat gallery)
    public NanoChatUiMessageEvent(NanoChatUiMessageType type,
        uint? recipientNumber = null,
        string? content = null,
        string? recipientJob = null, // Pirate: camera (nanochat gallery)
        string? photoFileName = null, // Pirate: camera (nanochat gallery)
        uint? messageId = null) // Pirate: camera (nanochat gallery)
    {
        Type = type;
        RecipientNumber = recipientNumber;
        Content = content;
        RecipientJob = recipientJob;
        PhotoFileName = photoFileName; // Pirate: camera (nanochat gallery)
        MessageId = messageId; // Pirate: camera (nanochat gallery)
    }
}

[Serializable, NetSerializable]
public enum NanoChatUiMessageType : byte
{
    NewChat,
    SelectChat,
    SelectGalleryPhoto, // Pirate: camera (nanochat gallery)
    CloseChat,
    SendMessage,
    DeleteChat,
    ToggleMute,
    ToggleListNumber,
    DeleteGalleryPhoto, // Pirate: camera (nanochat gallery)
    StoreMessagePhoto, // Pirate: camera (nanochat gallery)
}

// putting this here because i can
[Serializable, NetSerializable, DataRecord]
public partial struct NanoChatRecipient
{
    /// <summary>
    ///     The recipient's unique NanoChat number.
    /// </summary>
    public uint Number;

    /// <summary>
    ///     The recipient's display name, typically from their ID card.
    /// </summary>
    public string Name;

    /// <summary>
    ///     The recipient's job title, if available.
    /// </summary>
    public string? JobTitle;

    /// <summary>
    ///     Whether this recipient has unread messages.
    /// </summary>
    public bool HasUnread;

    /// <summary>
    ///     Creates a new NanoChat recipient.
    /// </summary>
    /// <param name="number">The recipient's NanoChat number</param>
    /// <param name="name">The recipient's display name</param>
    /// <param name="jobTitle">Optional job title for the recipient</param>
    /// <param name="hasUnread">Whether there are unread messages from this recipient</param>
    public NanoChatRecipient(uint number, string name, string? jobTitle = null, bool hasUnread = false)
    {
        Number = number;
        Name = name;
        JobTitle = jobTitle;
        HasUnread = hasUnread;
    }
}

#region Pirate: camera (nanochat gallery)

[Serializable, NetSerializable, DataRecord]
public partial struct NanoChatPhotoData
{
    public string FileName;
    public byte[]? ImageData;
    public byte[]? PreviewData;
    public string? Caption;
    public string? Description;
    public IReadOnlyList<string> NamesSeen;

    public NanoChatPhotoData(
        string fileName,
        byte[]? imageData,
        byte[]? previewData = null,
        string? caption = null,
        string? description = null,
        IReadOnlyList<string>? namesSeen = null)
    {
        FileName = fileName;
        ImageData = imageData;
        PreviewData = previewData;
        Caption = caption;
        Description = description;
        NamesSeen = namesSeen?.ToArray() ?? [];
    }
    #endregion
}

[Serializable, NetSerializable, DataRecord]
public partial struct NanoChatMessage
{
    public const int MaxContentLength = 256;

    /// <summary>
    ///     When the message was sent.
    /// </summary>
    public TimeSpan Timestamp;

    /// <summary>
    ///     The content of the message.
    /// </summary>
    public string Content;

    /// <summary>
    ///     The NanoChat number of the sender.
    /// </summary>
    public uint SenderId;

    /// <summary>
    ///     Whether the message failed to deliver to the recipient.
    ///     This can happen if the recipient is out of range or if there's no active telecomms server.
    /// </summary>
    public bool DeliveryFailed;

    #region Pirate: nullable nano chat message photo payload
    /// <summary>
    ///     Whether the message contains a photo attachment.
    /// </summary>
    public bool HasPhoto;

    /// <summary>
    ///     Attached photo payload, when present.
    /// </summary>
    public NanoChatPhotoData? Photo;
    /// <summary>
    ///     Unique id for this message on the local card copy.
    /// </summary>
    public uint Id;
    #endregion

    /// <summary>
    ///     Creates a new NanoChat message.
    /// </summary>
    /// <param name="id">Unique id for this message on the local card</param> // Pirate: camera (nanochat gallery)
    /// <param name="timestamp">When the message was sent</param>
    /// <param name="content">The content of the message</param>
    /// <param name="senderId">The sender's NanoChat number</param>
    /// <param name="deliveryFailed">Whether delivery to the recipient failed</param>
    /// <param name="photo">Optional photo attachment payload</param> // Pirate: camera (nanochat gallery)
    public NanoChatMessage(uint id, TimeSpan timestamp, string content, uint senderId, bool deliveryFailed = false, NanoChatPhotoData? photo = null) // Pirate: camera (nanochat gallery)
    {
        Timestamp = timestamp;
        Content = content;
        SenderId = senderId;
        DeliveryFailed = deliveryFailed;
        #region Pirate: camera (nanochat gallery)
        Id = id;
        HasPhoto = photo != null;
        Photo = photo;
        #endregion
    }
}

/// <summary>
///     NanoChat log data struct
/// </summary>
/// <remarks>Used by the LogProbe</remarks>
[Serializable, NetSerializable, DataRecord]
public readonly partial struct NanoChatData(
    Dictionary<uint, NanoChatRecipient> recipients,
    Dictionary<uint, List<NanoChatMessage>> messages,
    Dictionary<string, NanoChatPhotoData> photos, // Pirate: camera (nanochat gallery)
    uint? cardNumber,
    NetEntity card)
{
    public Dictionary<uint, NanoChatRecipient> Recipients { get; } = recipients;
    public Dictionary<uint, List<NanoChatMessage>> Messages { get; } = messages;
    public Dictionary<string, NanoChatPhotoData> Photos { get; } = photos; // Pirate: camera (nanochat gallery)
    public uint? CardNumber { get; } = cardNumber;
    public NetEntity Card { get; } = card;
}

/// <summary>
///     Raised on the NanoChat card whenever a recipient gets added
/// </summary>
[ByRefEvent]
public readonly record struct NanoChatRecipientUpdatedEvent(EntityUid CardUid);

/// <summary>
///     Raised on the NanoChat card whenever it receives or tries sending a messsage
/// </summary>
[ByRefEvent]
public readonly record struct NanoChatMessageReceivedEvent(EntityUid CardUid);
