// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._DV.CartridgeLoader.Cartridges;
using Content.Shared.Examine;
using Robust.Shared.Timing;

namespace Content.Shared._DV.NanoChat;

/// <summary>
///     Base system for NanoChat functionality shared between client and server.
/// </summary>
public abstract class SharedNanoChatSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    public const int MaxPhotos = 50; // Pirate: camera (nanochat gallery)

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NanoChatCardComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<NanoChatCardComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        if (ent.Comp.Number == null)
        {
            args.PushMarkup(Loc.GetString("nanochat-card-examine-no-number"));
            return;
        }

        args.PushMarkup(Loc.GetString("nanochat-card-examine-number", ("number", $"{ent.Comp.Number:D4}")));
    }

    #region Public API Methods

    /// <summary>
    ///     Gets the NanoChat number for a card.
    /// </summary>
    public uint? GetNumber(Entity<NanoChatCardComponent?> card)
    {
        if (!Resolve(card, ref card.Comp))
            return null;

        return card.Comp.Number;
    }

    /// <summary>
    ///     Sets the NanoChat number for a card.
    /// </summary>
    public void SetNumber(Entity<NanoChatCardComponent?> card, uint number)
    {
        if (!Resolve(card, ref card.Comp))
            return;

        card.Comp.Number = number;
        Dirty(card);
    }

    /// <summary>
    ///     Sets IsClosed for a card.
    /// </summary>
    public void SetClosed(Entity<NanoChatCardComponent?> card, bool closed)
    {
        if (!Resolve(card, ref card.Comp))
            return;

        card.Comp.IsClosed = closed;
    }

    /// <summary>
    ///     Gets the recipients dictionary from a card.
    /// </summary>
    public IReadOnlyDictionary<uint, NanoChatRecipient> GetRecipients(Entity<NanoChatCardComponent?> card)
    {
        if (!Resolve(card, ref card.Comp))
            return new Dictionary<uint, NanoChatRecipient>();

        return card.Comp.Recipients;
    }

    /// <summary>
    ///     Gets the messages dictionary from a card.
    /// </summary>
    public IReadOnlyDictionary<uint, List<NanoChatMessage>> GetMessages(Entity<NanoChatCardComponent?> card)
    {
        if (!Resolve(card, ref card.Comp))
            return new Dictionary<uint, List<NanoChatMessage>>();

        return card.Comp.Messages;
    }

    /// <summary>
    ///     Sets a specific recipient in the card.
    /// </summary>
    public void SetRecipient(Entity<NanoChatCardComponent?> card, uint number, NanoChatRecipient recipient)
    {
        if (!Resolve(card, ref card.Comp))
            return;

        card.Comp.Recipients[number] = recipient;
        Dirty(card);
    }

    /// <summary>
    ///     Gets a specific recipient from the card.
    /// </summary>
    public NanoChatRecipient? GetRecipient(Entity<NanoChatCardComponent?> card, uint number)
    {
        if (!Resolve(card, ref card.Comp) || !card.Comp.Recipients.TryGetValue(number, out var recipient))
            return null;

        return recipient;
    }

    /// <summary>
    ///     Gets all messages for a specific recipient.
    /// </summary>
    public List<NanoChatMessage>? GetMessagesForRecipient(Entity<NanoChatCardComponent?> card, uint recipientNumber)
    {
        if (!Resolve(card, ref card.Comp) || !card.Comp.Messages.TryGetValue(recipientNumber, out var messages))
            return null;

        return new List<NanoChatMessage>(messages);
    }

    /// <summary>
    ///     Adds a message to a recipient's conversation.
    /// </summary>
    public void AddMessage(Entity<NanoChatCardComponent?> card, uint recipientNumber, NanoChatMessage message)
    {
        if (!Resolve(card, ref card.Comp))
            return;

        if (!card.Comp.Messages.TryGetValue(recipientNumber, out var messages))
        {
            messages = new List<NanoChatMessage>();
            card.Comp.Messages[recipientNumber] = messages;
        }

        messages.Add(message);
        card.Comp.LastMessageTime = _timing.CurTime;
        Dirty(card);
    }

    /// <summary>
    ///     Gets the currently selected chat recipient.
    /// </summary>
    public uint? GetCurrentChat(Entity<NanoChatCardComponent?> card)
    {
        if (!Resolve(card, ref card.Comp))
            return null;

        return card.Comp.CurrentChat;
    }

    /// <summary>
    ///     Sets the currently selected chat recipient.
    /// </summary>
    public void SetCurrentChat(Entity<NanoChatCardComponent?> card, uint? recipient)
    {
        if (!Resolve(card, ref card.Comp))
            return;

        card.Comp.CurrentChat = recipient;
        Dirty(card);
    }

    /// <summary>
    ///     Gets whether notifications are muted.
    /// </summary>
    public bool GetNotificationsMuted(Entity<NanoChatCardComponent?> card)
    {
        if (!Resolve(card, ref card.Comp))
            return false;

        return card.Comp.NotificationsMuted;
    }

    /// <summary>
    ///     Sets whether notifications are muted.
    /// </summary>
    public void SetNotificationsMuted(Entity<NanoChatCardComponent?> card, bool muted)
    {
        if (!Resolve(card, ref card.Comp))
            return;

        card.Comp.NotificationsMuted = muted;
        Dirty(card);
    }

    /// <summary>
    ///     Gets whether NanoChat number is listed.
    /// </summary>
    public bool GetListNumber(Entity<NanoChatCardComponent?> card)
    {
        if (!Resolve(card, ref card.Comp))
            return false;

        return card.Comp.ListNumber;
    }

    /// <summary>
    ///     Sets whether NanoChat number is listed.
    /// </summary>
    public void SetListNumber(Entity<NanoChatCardComponent?> card, bool listNumber)
    {
        if (!Resolve(card, ref card.Comp) || card.Comp.ListNumber == listNumber)
            return;

        card.Comp.ListNumber = listNumber;
        Dirty(card);
    }

    /// <summary>
    ///     Gets the time of the last message.
    /// </summary>
    public TimeSpan? GetLastMessageTime(Entity<NanoChatCardComponent?> card)
    {
        if (!Resolve(card, ref card.Comp))
            return null;

        return card.Comp.LastMessageTime;
    }

    /// <summary>
    ///     Gets if there are unread messages from a recipient.
    /// </summary>
    public bool HasUnreadMessages(Entity<NanoChatCardComponent?> card, uint recipientNumber)
    {
        if (!Resolve(card, ref card.Comp) || !card.Comp.Recipients.TryGetValue(recipientNumber, out var recipient))
            return false;

        return recipient.HasUnread;
    }

    #region Pirate: camera (nanochat gallery)
    /// <summary>
    ///     Allocates the next local message id for this card.
    /// </summary>
    public uint AllocateMessageId(Entity<NanoChatCardComponent?> card)
    {
        if (!Resolve(card, ref card.Comp))
            return 0;

        var id = card.Comp.NextMessageId++;
        Dirty(card);
        return id;
    }
    /// <summary>
    ///     Gets the stored PDA gallery photos for a card.
    /// </summary>
    public IReadOnlyDictionary<string, NanoChatPhotoData> GetStoredPhotos(Entity<NanoChatCardComponent?> card)
    {
        if (!Resolve(card, ref card.Comp))
            return new Dictionary<string, NanoChatPhotoData>();

        return card.Comp.Photos;
    }
    /// <summary>
    ///     Gets the currently selected gallery photo file name for external actions.
    /// </summary>
    public string? GetSelectedGalleryPhoto(Entity<NanoChatCardComponent?> card)
    {
        if (!Resolve(card, ref card.Comp))
            return null;

        return card.Comp.SelectedGalleryPhotoFileName;
    }

    /// <summary>
    ///     Sets the currently selected gallery photo file name for external actions.
    /// </summary>
    public void SetSelectedGalleryPhoto(Entity<NanoChatCardComponent?> card, string? fileName)
    {
        if (!Resolve(card, ref card.Comp) || card.Comp.SelectedGalleryPhotoFileName == fileName)
            return;

        card.Comp.SelectedGalleryPhotoFileName = fileName;
        Dirty(card);
    }

    /// <summary>
    ///     Tries to store or overwrite a gallery photo by file name.
    /// </summary>
    public bool TryStorePhoto(Entity<NanoChatCardComponent?> card, NanoChatPhotoData photo)
    {
        if (!Resolve(card, ref card.Comp) || string.IsNullOrWhiteSpace(photo.FileName))
            return false;

        if (!card.Comp.Photos.ContainsKey(photo.FileName) && card.Comp.Photos.Count >= MaxPhotos)
            return false;

        card.Comp.Photos[photo.FileName] = photo;
        Dirty(card);
        return true;
    }

    /// <summary>
    ///     Tries to get a stored gallery photo by file name.
    /// </summary>
    public bool TryGetStoredPhoto(Entity<NanoChatCardComponent?> card, string fileName, out NanoChatPhotoData photo)
    {
        photo = default;
        return Resolve(card, ref card.Comp) && card.Comp.Photos.TryGetValue(fileName, out photo);
    }

    /// <summary>
    ///     Deletes a stored gallery photo by file name.
    /// </summary>
    public bool TryDeleteStoredPhoto(Entity<NanoChatCardComponent?> card, string fileName)
    {
        if (!Resolve(card, ref card.Comp))
            return false;

        var deleted = card.Comp.Photos.Remove(fileName);
        if (deleted)
        {
            if (card.Comp.SelectedGalleryPhotoFileName == fileName)
                card.Comp.SelectedGalleryPhotoFileName = null;
            Dirty(card);
        }

        return deleted;
    }
    #endregion

    /// <summary>
    ///     Clears all messages and recipients from the card.
    /// </summary>
    public void Clear(Entity<NanoChatCardComponent?> card)
    {
        if (!Resolve(card, ref card.Comp))
            return;

        card.Comp.Messages.Clear();
        card.Comp.Recipients.Clear();
        card.Comp.Photos.Clear(); // Pirate: camera (nanochat gallery)
        card.Comp.CurrentChat = null;
        card.Comp.SelectedGalleryPhotoFileName = null; // Pirate: camera (nanochat gallery)
        Dirty(card);
    }

    /// <summary>
    ///     Deletes a chat conversation with a recipient from the card.
    ///     Optionally keeps message history while removing from active chats.
    /// </summary>
    /// <returns>True if the chat was deleted successfully</returns>
    public bool TryDeleteChat(Entity<NanoChatCardComponent?> card, uint recipientNumber, bool keepMessages = false)
    {
        if (!Resolve(card, ref card.Comp))
            return false;

        // Remove from recipients list
        var removed = card.Comp.Recipients.Remove(recipientNumber);

        // Clear messages if requested
        if (!keepMessages)
            card.Comp.Messages.Remove(recipientNumber);

        // Clear current chat if we just deleted it
        if (card.Comp.CurrentChat == recipientNumber)
            card.Comp.CurrentChat = null;

        if (removed)
            Dirty(card);

        return removed;
    }

    /// <summary>
    ///     Ensures a recipient exists in the card's contacts and message lists.
    ///     If the recipient doesn't exist, they will be added with the provided info.
    /// </summary>
    /// <returns>True if the recipient was added or already existed</returns>
    public bool EnsureRecipientExists(Entity<NanoChatCardComponent?> card,
        uint recipientNumber,
        NanoChatRecipient? recipientInfo = null)
    {
        if (!Resolve(card, ref card.Comp))
            return false;

        var changed = false; // Pirate: pda fix
        if (!card.Comp.Recipients.ContainsKey(recipientNumber))
        {
            // Only add if we have recipient info
            if (recipientInfo == null)
                return false;

            card.Comp.Recipients[recipientNumber] = recipientInfo.Value;
            changed = true; // Pirate: pda fix
        }
        #region Pirate: pda fix
        else if (recipientInfo is { } info)
        {
            // Enrich existing recipient data when we learn missing fields later.
            var existing = card.Comp.Recipients[recipientNumber];
            var updated = new NanoChatRecipient(existing.Number, existing.Name, existing.JobTitle, existing.HasUnread);

            if (string.IsNullOrWhiteSpace(existing.Name) && !string.IsNullOrWhiteSpace(info.Name))
                updated.Name = info.Name;

            if (string.IsNullOrWhiteSpace(existing.JobTitle) && !string.IsNullOrWhiteSpace(info.JobTitle))
                updated.JobTitle = info.JobTitle;

            if (!updated.Equals(existing))
            {
                card.Comp.Recipients[recipientNumber] = updated;
                changed = true;
            }
        }

        // Ensure message list exists for this recipient
        if (!card.Comp.Messages.ContainsKey(recipientNumber))
        {
            card.Comp.Messages[recipientNumber] = new List<NanoChatMessage>();
            changed = true;
        }

        if (changed)
            Dirty(card);
        #endregion

        return true;
    }

    #endregion
}

