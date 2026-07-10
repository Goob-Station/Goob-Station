// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server.Administration.Logs;
using Content.Server.CartridgeLoader;
using Content.Server.Power.Components;
using Content.Server.Radio;
using Content.Shared.Radio.Components;
using Content.Server.Station.Systems;
using Content.Shared.Access.Components;
using Content.Shared.CartridgeLoader;
using Content.Shared.Database;
using Content.Shared._DV.CartridgeLoader.Cartridges;
using Content.Shared._DV.NanoChat;
using Content.Shared.PDA;
using Content.Shared.Radio.Components;
using Robust.Shared.Audio; // Pirate: pda fix
using Robust.Shared.Audio.Systems; // Pirate: pda fix
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Utility; // Goob
#region Pirate: camera (nanochat gallery)
using Content.Shared.Verbs;
using Content.Shared._Pirate.NanoChat;
using System.IO;
using Content.Server.DoAfter;
using Content.Server.Fax;
using Content.Server.Popups;
using Content.Server._Pirate.Photo;
using Content.Shared.GameTicking;
using Content.Shared.DoAfter;
using Content.Shared.Fax.Components;
#endregion

namespace Content.Server._DV.CartridgeLoader.Cartridges;

public sealed class NanoChatCartridgeSystem : EntitySystem
{
    [Dependency] private readonly CartridgeLoaderSystem _cartridge = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly SharedNanoChatSystem _nanoChat = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly IConfigurationManager _cfgManager = default!;
    #region Pirate: camera (nanochat gallery)
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly FaxSystem _fax = default!;
    [Dependency] private readonly SharedGameTicker _gameTicker = default!;
    #endregion

    // Messages in notifications get cut off after this point
    // no point in storing it on the comp
    private const int NotificationMaxLength = 64;
    #region Pirate: pda fix
    private static readonly TimeSpan PhotoUploadDelay = TimeSpan.FromSeconds(0.8);
    private static readonly SoundSpecifier SendSuccessSound = new SoundPathSpecifier("/Audio/_Pirate/Machines/terminal_success.ogg");
    private static readonly SoundSpecifier SendErrorSound = new SoundPathSpecifier("/Audio/_Pirate/Machines/terminal_error.ogg");
    private static readonly SoundSpecifier PhotoScanSuccessSound = new SoundPathSpecifier("/Audio/_Pirate/Machines/terminal_prompt_confirm.ogg");
    private static readonly SoundSpecifier RecipientMessageSound = new SoundPathSpecifier("/Audio/Machines/twobeep.ogg");
    private static readonly AudioParams RecipientMessageAudioParams = AudioParams.Default
        .WithVolume(2f)
        .WithMaxDistance(5f)
        .WithPitchScale(1.2f);
    private static readonly AudioParams SenderFeedbackAudioParams = AudioParams.Default
        .WithVolume(1.5f)
        .WithMaxDistance(5f);
    #endregion

    private int _maxNameLength;
    private int _maxIdJobLength;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NanoChatCartridgeComponent, CartridgeUiReadyEvent>(OnUiReady);
        SubscribeLocalEvent<NanoChatCartridgeComponent, CartridgeMessageEvent>(OnMessage);
        SubscribeLocalEvent<PdaComponent, PdaPhotoUploadDoAfterEvent>(OnPdaPhotoUploadDoAfter);
        SubscribeLocalEvent<PdaComponent, GetVerbsEvent<AlternativeVerb>>(OnPdaGetAlternativeVerbs); // Pirate: nano chat photo scan sound
        SubscribeLocalEvent<FaxMachineComponent, GetVerbsEvent<AlternativeVerb>>(OnFaxGetAlternativeVerbs); // Pirate: nano chat photo scan sound
        SubscribeLocalEvent<FaxMachineComponent, PdaPhotoPrintToFaxDoAfterEvent>(OnFaxPhotoPrintToFaxDoAfter); // Pirate: nano chat photo scan sound

        Subs.CVar(_cfgManager, CCVars.MaxNameLength, value => _maxNameLength = value, true);
        Subs.CVar(_cfgManager, CCVars.MaxIdJobLength, value => _maxIdJobLength = value, true);
    }

    private void UpdateClosed(Entity<NanoChatCartridgeComponent> ent)
    {
        if (!TryComp<CartridgeComponent>(ent, out var cartridge) ||
            cartridge.LoaderUid is not { } pda ||
            !TryComp<CartridgeLoaderComponent>(pda, out var loader) ||
            !GetCardEntity(pda, out var card))
        {
            return;
        }

        // if you switch to another program or close the pda UI, allow notifications for the selected chat
        _nanoChat.SetClosed((card, card.Comp), loader.ActiveProgram != ent.Owner || !_ui.IsUiOpen(pda, PdaUiKey.Key));
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Update card references for any cartridges that need it
        var query = EntityQueryEnumerator<NanoChatCartridgeComponent, CartridgeComponent>();
        while (query.MoveNext(out var uid, out var nanoChat, out var cartridge))
        {
            if (cartridge.LoaderUid == null)
                continue;

            UpdateClosed((uid, nanoChat)); // Pirate: pda fix
            // Check if we need to update our card reference
            if (!TryComp<PdaComponent>(cartridge.LoaderUid, out var pda))
                continue;

            var newCard = pda.ContainedId;
            var currentCard = nanoChat.Card;

            // If the cards match, nothing to do
            if (newCard == currentCard)
                continue;

            // Update card reference
            nanoChat.Card = newCard;

            // Update UI state since card reference changed
            UpdateUI((uid, nanoChat), cartridge.LoaderUid.Value);
        }
    }

    /// <summary>
    ///     Handles incoming UI messages from the NanoChat cartridge.
    /// </summary>
    private void OnMessage(Entity<NanoChatCartridgeComponent> ent, ref CartridgeMessageEvent args)
    {
        if (args is not NanoChatUiMessageEvent msg)
            return;

        if (!GetCardEntity(GetEntity(args.LoaderUid), out var card))
            return;

        switch (msg.Type)
        {
            case NanoChatUiMessageType.NewChat:
                HandleNewChat(card, msg);
                break;
            case NanoChatUiMessageType.SelectChat:
                HandleSelectChat(card, msg);
                break;
            case NanoChatUiMessageType.CloseChat:
                HandleCloseChat(card);
                break;
            case NanoChatUiMessageType.ToggleMute:
                HandleToggleMute(card);
                break;
            case NanoChatUiMessageType.DeleteChat:
                HandleDeleteChat(card, msg);
                break;
            case NanoChatUiMessageType.SendMessage:
                HandleSendMessage(ent, card, msg);
                break;
            case NanoChatUiMessageType.ToggleListNumber:
                HandleToggleListNumber(card);
                break;
            #region Pirate: camera (nanochat gallery)
            case NanoChatUiMessageType.DeleteGalleryPhoto:
                HandleDeleteGalleryPhoto(card, msg);
                break;
            case NanoChatUiMessageType.StoreMessagePhoto:
                HandleStoreMessagePhoto(card, msg);
                break;
            case NanoChatUiMessageType.SelectGalleryPhoto:
                HandleSelectGalleryPhoto(card, msg);
                break;
            #endregion
        }

        UpdateUI(ent, GetEntity(args.LoaderUid));
    }

    /// <summary>
    ///     Gets the ID card entity associated with a PDA.
    /// </summary>
    /// <param name="loaderUid">The PDA entity ID</param>
    /// <param name="card">Output parameter containing the found card entity and component</param>
    /// <returns>True if a valid NanoChat card was found</returns>
    private bool GetCardEntity(
        EntityUid loaderUid,
        out Entity<NanoChatCardComponent> card)
    {
        card = default;

        // Get the PDA and check if it has an ID card
        if (!TryComp<PdaComponent>(loaderUid, out var pda) ||
            pda.ContainedId == null ||
            !TryComp<NanoChatCardComponent>(pda.ContainedId, out var idCard))
            return false;

        card = (pda.ContainedId.Value, idCard);
        return true;
    }

    /// <summary>
    ///     Handles creation of a new chat conversation.
    /// </summary>
    private void HandleNewChat(Entity<NanoChatCardComponent> card, NanoChatUiMessageEvent msg)
    {
        if (msg.RecipientNumber == null || msg.Content == null || msg.RecipientNumber == card.Comp.Number)
            return;

        var name = msg.Content;
        if (!string.IsNullOrWhiteSpace(name))
        {
            name = name.Trim();
            if (name.Length > _maxNameLength)
                name = name[.._maxNameLength];
        }

        var jobTitle = msg.RecipientJob;
        if (!string.IsNullOrWhiteSpace(jobTitle))
        {
            jobTitle = jobTitle.Trim();
            if (jobTitle.Length > _maxIdJobLength)
                jobTitle = jobTitle[.._maxIdJobLength];
        }

        // Add new recipient
        var recipient = new NanoChatRecipient(msg.RecipientNumber.Value,
            name,
            jobTitle);

        // Initialize or update recipient
        _nanoChat.SetRecipient((card, card.Comp), msg.RecipientNumber.Value, recipient);

        _adminLogger.Add(LogType.Action,
            LogImpact.Low,
            $"{ToPrettyString(msg.Actor):user} created new NanoChat conversation with #{msg.RecipientNumber:D4} ({name})");

        var recipientEv = new NanoChatRecipientUpdatedEvent(card);
        RaiseLocalEvent(ref recipientEv);
        UpdateUIForCard(card);
    }

    /// <summary>
    ///     Handles selecting a chat conversation.
    /// </summary>
    private void HandleSelectChat(Entity<NanoChatCardComponent> card, NanoChatUiMessageEvent msg)
    {
        if (msg.RecipientNumber == null)
            return;

        _nanoChat.SetCurrentChat((card, card.Comp), msg.RecipientNumber);

        // Clear unread flag when selecting chat
        if (_nanoChat.GetRecipient((card, card.Comp), msg.RecipientNumber.Value) is { } recipient)
        {
            _nanoChat.SetRecipient((card, card.Comp),
                msg.RecipientNumber.Value,
                recipient with { HasUnread = false });
        }
    }

    /// <summary>
    ///     Handles closing the current chat conversation.
    /// </summary>
    private void HandleCloseChat(Entity<NanoChatCardComponent> card)
    {
        _nanoChat.SetCurrentChat((card, card.Comp), null);
    }

    /// <summary>
    ///     Handles deletion of a chat conversation.
    /// </summary>
    private void HandleDeleteChat(Entity<NanoChatCardComponent> card, NanoChatUiMessageEvent msg)
    {
        if (msg.RecipientNumber == null || card.Comp.Number == null)
            return;

        // Delete chat but keep the messages
        var deleted = _nanoChat.TryDeleteChat((card, card.Comp), msg.RecipientNumber.Value, true);

        if (!deleted)
            return;

        _adminLogger.Add(LogType.Action,
            LogImpact.Low,
            $"{ToPrettyString(msg.Actor):user} deleted NanoChat conversation with #{msg.RecipientNumber:D4}");

        UpdateUIForCard(card);
    }

    /// <summary>
    ///     Handles toggling notification mute state.
    /// </summary>
    private void HandleToggleMute(Entity<NanoChatCardComponent> card)
    {
        _nanoChat.SetNotificationsMuted((card, card.Comp), !_nanoChat.GetNotificationsMuted((card, card.Comp)));
        UpdateUIForCard(card);
    }

    private void HandleToggleListNumber(Entity<NanoChatCardComponent> card)
    {
        _nanoChat.SetListNumber((card, card.Comp), !_nanoChat.GetListNumber((card, card.Comp)));
        UpdateUIForAllCards();
    }

    /// <summary>
    ///     Handles sending a new message in a chat conversation.
    /// </summary>
    private void HandleSendMessage(Entity<NanoChatCartridgeComponent> cartridge,
        Entity<NanoChatCardComponent> card,
        NanoChatUiMessageEvent msg)
    {
        if (msg.RecipientNumber == null || card.Comp.Number == null)  // Pirate: camera (nanochat gallery)
            return;

        if (!EnsureRecipientExists(card, msg.RecipientNumber.Value)) // Pirate: pda fix
            return;

        #region
        NanoChatPhotoData? attachment = null;
        if (!string.IsNullOrWhiteSpace(msg.PhotoFileName))
        {
            if (!_nanoChat.TryGetStoredPhoto((card, card.Comp), msg.PhotoFileName, out var storedPhoto))
                return;

            attachment = storedPhoto;
        }
        var content = msg.Content ?? string.Empty;
        #endregion

        if (!string.IsNullOrWhiteSpace(content))
        {
            content = FormattedMessage.EscapeText(content.Trim()); // Goob Sanitize Text
            if (content.Length > NanoChatMessage.MaxContentLength)
                content = content[..NanoChatMessage.MaxContentLength];
        }
        else // Pirate: camera (nanochat gallery)
        {
            content = string.Empty; // Pirate: camera (nanochat gallery)
        }

        if (string.IsNullOrWhiteSpace(content) && attachment == null) // Pirate: camera (nanochat gallery)
            return; // Pirate: camera (nanochat gallery)

        // Create and store message for sender
        var message = new NanoChatMessage(
            _nanoChat.AllocateMessageId((card, card.Comp)), // Pirate: camera (nanochat gallery)
            _timing.CurTime,
            content,
            (uint)card.Comp.Number, // Pirate: camera (nanochat gallery)
            photo: attachment // Pirate: camera (nanochat gallery)
        );

        // Attempt delivery
        var (deliveryFailed, recipients) = AttemptMessageDelivery(cartridge, msg.RecipientNumber.Value);

        // Update delivery status
        message = message with { DeliveryFailed = deliveryFailed };

        // Store message in sender's outbox under recipient's number
        _nanoChat.AddMessage((card, card.Comp), msg.RecipientNumber.Value, message);

        // Log message attempt
        var recipientsText = recipients.Count > 0
            ? string.Join(", ", recipients.Select(r => ToPrettyString(r)))
            : $"#{msg.RecipientNumber:D4}";
        var attachmentText = attachment is { } photo ? $" [PHOTO: {photo.FileName}]" : string.Empty; // Pirate: camera (nanochat gallery)

        _adminLogger.Add(LogType.Chat,
            LogImpact.Low,
            $"{ToPrettyString(msg.Actor):user} sent NanoChat message to {recipientsText}: {content}{attachmentText}{(deliveryFailed ? " [DELIVERY FAILED]" : "")}"); // Pirate: camera (nanochat gallery)

        var msgEv = new NanoChatMessageReceivedEvent(card);
        RaiseLocalEvent(ref msgEv);

        if (deliveryFailed || !card.Comp.NotificationsMuted) // Pirate: pda fix
            PlaySenderFeedbackSound((card, card.Comp), deliveryFailed); // Pirate: pda fix

        if (deliveryFailed)
            return;

        foreach (var recipient in recipients)
        {
            DeliverMessageToRecipient(card, recipient, message);
        }
    }

    #region Pirate: pda fix
    private void PlaySenderFeedbackSound(Entity<NanoChatCardComponent> card, bool deliveryFailed)
    {
        var source = card.Comp.PdaUid ?? card.Owner;
        if (Deleted(source))
            return;

        _audio.PlayPvs(deliveryFailed ? SendErrorSound : SendSuccessSound, source, SenderFeedbackAudioParams);
    }
    #endregion

    /// <summary>
    ///     Ensures a recipient exists in the sender's contacts.
    /// </summary>
    /// <param name="card">The card to check contacts for</param>
    /// <param name="recipientNumber">The recipient's number to check</param>
    /// <returns>True if the recipient exists or was created successfully</returns>
    private bool EnsureRecipientExists(Entity<NanoChatCardComponent> card, uint recipientNumber, NanoChatRecipient? recipientInfo = null) // Pirate: pda fix
    {
        return _nanoChat.EnsureRecipientExists((card, card.Comp), recipientNumber, recipientInfo ?? GetCardInfo(recipientNumber)); // Pirate: pda fix
    }

    /// <summary>
    ///     Attempts to deliver a message to recipients.
    /// </summary>
    /// <param name="sender">The sending cartridge entity</param>
    /// <param name="recipientNumber">The recipient's number</param>
    /// <returns>Tuple containing delivery status and recipients if found.</returns>
    private (bool failed, List<Entity<NanoChatCardComponent>> recipient) AttemptMessageDelivery(
        Entity<NanoChatCartridgeComponent> sender,
        uint recipientNumber)
    {
        // First verify we can send from this device
        var channel = _prototype.Index(sender.Comp.RadioChannel);
        var sendAttemptEvent = new RadioSendAttemptEvent(channel, sender);
        RaiseLocalEvent(ref sendAttemptEvent);
        if (sendAttemptEvent.Cancelled)
            return (true, new List<Entity<NanoChatCardComponent>>());

        var foundRecipients = new List<Entity<NanoChatCardComponent>>();

        // Find all cards with matching number
        var cardQuery = EntityQueryEnumerator<NanoChatCardComponent>();
        while (cardQuery.MoveNext(out var cardUid, out var card))
        {
            if (card.Number != recipientNumber)
                continue;

            foundRecipients.Add((cardUid, card));
        }

        if (foundRecipients.Count == 0)
            return (true, foundRecipients);

        // Now check if any of these cards can receive
        var deliverableRecipients = new List<Entity<NanoChatCardComponent>>();
        foreach (var recipient in foundRecipients)
        {
            // Find any cartridges that have this card
            var cartridgeQuery = EntityQueryEnumerator<NanoChatCartridgeComponent, ActiveRadioComponent>();
            while (cartridgeQuery.MoveNext(out var receiverUid, out var receiverCart, out _))
            {
                if (receiverCart.Card != recipient.Owner)
                    continue;

                // Check if devices are on same station/map
                var recipientStation = _station.GetOwningStation(receiverUid);
                var senderStation = _station.GetOwningStation(sender);

                // Both entities must be on a station
                if (recipientStation == null || senderStation == null)
                    continue;

                // Must be on same map/station unless long range allowed
                if (!channel.LongRange && recipientStation != senderStation)
                    continue;

                // Needs telecomms
                if (!HasActiveServer(senderStation.Value) || !HasActiveServer(recipientStation.Value))
                    continue;

                // Check if recipient can receive
                var receiveAttemptEv = new RadioReceiveAttemptEvent(channel, sender, receiverUid);
                RaiseLocalEvent(ref receiveAttemptEv);
                if (receiveAttemptEv.Cancelled)
                    continue;

                // Found valid cartridge that can receive
                deliverableRecipients.Add(recipient);
                break; // Only need one valid cartridge per card
            }
        }

        return (deliverableRecipients.Count == 0, deliverableRecipients);
    }

    /// <summary>
    ///     Checks if there are any active telecomms servers on the given station
    /// </summary>
    private bool HasActiveServer(EntityUid station)
    {
        // I have no idea why this isn't public in the RadioSystem
        var query =
            EntityQueryEnumerator<TelecomServerComponent, EncryptionKeyHolderComponent, ApcPowerReceiverComponent>();

        while (query.MoveNext(out var uid, out _, out _, out var power))
        {
            if (_station.GetOwningStation(uid) == station && power.Powered)
                return true;
        }

        return false;
    }

    /// <summary>
    ///     Delivers a message to the recipient and handles associated notifications.
    /// </summary>
    /// <param name="sender">The sender's card entity</param>
    /// <param name="recipient">The recipient's card entity</param>
    /// <param name="message">The <see cref="NanoChatMessage" /> to deliver</param>
    private void DeliverMessageToRecipient(Entity<NanoChatCardComponent> sender,
        Entity<NanoChatCardComponent> recipient,
        NanoChatMessage message)
    {
        var senderNumber = sender.Comp.Number;
        if (senderNumber == null)
            return;

        // Always try to get and add sender info to recipient's contacts
        if (!EnsureRecipientExists(recipient, senderNumber.Value))
            return;

        #region Pirate: pda fix
        var shouldNotifyUnread = recipient.Comp.IsClosed || _nanoChat.GetCurrentChat((recipient, recipient.Comp)) != senderNumber;

        if (!recipient.Comp.NotificationsMuted && shouldNotifyUnread)
            _audio.PlayPvs(RecipientMessageSound, recipient.Comp.PdaUid ?? recipient.Owner, RecipientMessageAudioParams);
        #endregion
        #region Pirate: camera (nanochat gallery)
        var deliveredMessage = message with
        {
            Id = _nanoChat.AllocateMessageId((recipient, recipient.Comp)),
            DeliveryFailed = false
        };
        _nanoChat.AddMessage((recipient, recipient.Comp), senderNumber.Value, deliveredMessage); // Pirate: camera (nanochat gallery)
        #endregion

        if (shouldNotifyUnread) // Pirate: pda fix
            HandleUnreadNotification(recipient, deliveredMessage, (uint)senderNumber); // Pirate: camera(nanochat gallery)

        var msgEv = new NanoChatMessageReceivedEvent(recipient);
        RaiseLocalEvent(ref msgEv);
        UpdateUIForCard(recipient);
    }

    /// <summary>
    ///     Handles unread message notifications and updates unread status.
    /// </summary>
    private void HandleUnreadNotification(Entity<NanoChatCardComponent> recipient,
        NanoChatMessage message,
        uint senderNumber)
    {
        // Get sender name from contacts or fall back to number
        var recipients = _nanoChat.GetRecipients((recipient, recipient.Comp));
        var senderName = recipients.TryGetValue(message.SenderId, out var senderRecipient)
            ? senderRecipient.Name
            : $"#{message.SenderId:D4}";
        var hasSelectedCurrentChat = _nanoChat.GetCurrentChat((recipient, recipient.Comp)) == senderNumber;

        // Update unread status
        if (!hasSelectedCurrentChat)
            _nanoChat.SetRecipient((recipient, recipient.Comp),
                message.SenderId,
                senderRecipient with { HasUnread = true });

        if (recipient.Comp.PdaUid is not { } pdaUid || // Pirate: pda fix
            !TryComp<CartridgeLoaderComponent>(pdaUid, out var loader) ||
            // Don't notify if the recipient has the NanoChat program open with this chat selected.
            (hasSelectedCurrentChat &&
                _ui.IsUiOpen(pdaUid, PdaUiKey.Key) &&
                HasComp<NanoChatCartridgeComponent>(loader.ActiveProgram)))
            return;

        _cartridge.SendNotification(pdaUid,
            Loc.GetString("nano-chat-new-message-title", ("sender", senderName)),
            BuildNotificationBody(message), // Pirate: camera (nanochat gallery)
            loader, // Pirate: pda fix
            playRingtone: false); // Pirate: pda fix

    }

    /// <summary>
    ///     Updates the UI for any PDAs containing the specified card.
    /// </summary>
    private void UpdateUIForCard(EntityUid cardUid)
    {
        // Find any PDA containing this card and update its UI
        var query = EntityQueryEnumerator<NanoChatCartridgeComponent, CartridgeComponent>();
        while (query.MoveNext(out var uid, out var comp, out var cartridge))
        {
            if (comp.Card != cardUid || cartridge.LoaderUid == null)
                continue;

            UpdateUI((uid, comp), cartridge.LoaderUid.Value);
        }
    }

    /// <summary>
    ///     Updates the UI for all PDAs containing a NanoChat cartridge.
    /// </summary>
    private void UpdateUIForAllCards()
    {
        // Find any PDA containing this card and update its UI
        var query = EntityQueryEnumerator<NanoChatCartridgeComponent, CartridgeComponent>();
        while (query.MoveNext(out var uid, out var comp, out var cartridge))
        {
            if (cartridge.LoaderUid is { } loader)
                UpdateUI((uid, comp), loader);
        }
    }

    /// <summary>
    ///     Gets the <see cref="NanoChatRecipient" /> for a given NanoChat number.
    /// </summary>
    private NanoChatRecipient? GetCardInfo(uint number)
    {
        // Find card with this number to get its info
        var query = EntityQueryEnumerator<NanoChatCardComponent>();
        while (query.MoveNext(out var uid, out var card))
        {
            if (card.Number != number)
                continue;

            // Try to get job title from ID card if possible
            string? jobTitle = null;
            var name = "Unknown";
            if (TryComp<IdCardComponent>(uid, out var idCard))
            {
                jobTitle = idCard.LocalizedJobTitle;
                name = idCard.FullName ?? name;
            }

            return new NanoChatRecipient(number, name, jobTitle);
        }

        return null;
    }

    /// <summary>
    ///     Truncates a message to the notification maximum length.
    /// </summary>
    private static string TruncateMessage(string message)
    {
        return message.Length <= NotificationMaxLength
            ? message
            : message[..(NotificationMaxLength - 4)] + " [...]";
    }

    private void OnUiReady(Entity<NanoChatCartridgeComponent> ent, ref CartridgeUiReadyEvent args)
    {
        _cartridge.RegisterBackgroundProgram(args.Loader, ent);
        UpdateUI(ent, args.Loader);
    }

    private void UpdateUI(Entity<NanoChatCartridgeComponent> ent, EntityUid loader)
    {
        List<NanoChatRecipient>? contacts;
        if (_station.GetOwningStation(loader) is { } station)
        {
            ent.Comp.Station = station;

            contacts = [];

            var query = AllEntityQuery<NanoChatCardComponent, IdCardComponent>();
            while (query.MoveNext(out var entityId, out var nanoChatCard, out var idCardComponent))
            {
                if (nanoChatCard.ListNumber && nanoChatCard.Number is uint nanoChatNumber && idCardComponent.FullName is string fullName && _station.GetOwningStation(entityId) == station)
                {
                    contacts.Add(new NanoChatRecipient(nanoChatNumber, fullName, idCardComponent.LocalizedJobTitle)); // Pirate: pda fix
                }
            }
            contacts.Sort((contactA, contactB) => string.CompareOrdinal(contactA.Name, contactB.Name));
        }
        else
        {
            contacts = null;
        }

        var recipients = new Dictionary<uint, NanoChatRecipient>();
        var messages = new Dictionary<uint, List<NanoChatMessage>>();
        var photos = new Dictionary<string, NanoChatPhotoData>(); // Pirate: camera (nanochat gallery)
        uint? currentChat = null;
        uint ownNumber = 0;
        var maxRecipients = 50;
        var notificationsMuted = false;
        var listNumber = false;

        if (ent.Comp.Card != null && TryComp<NanoChatCardComponent>(ent.Comp.Card, out var card))
        {
            recipients = card.Recipients;
            messages = BuildUiMessages(card.Messages); // Pirate: camera (nanochat gallery)
            photos = BuildUiPhotos(card.Photos); // Pirate: camera (nanochat gallery)
            currentChat = card.CurrentChat;
            ownNumber = card.Number ?? 0;
            maxRecipients = card.MaxRecipients;
            notificationsMuted = card.NotificationsMuted;
            listNumber = card.ListNumber;
        }

        var state = new NanoChatUiState(recipients,
            messages,
            photos, // Pirate: camera (nanochat gallery)
            contacts,
            currentChat,
            ownNumber,
            maxRecipients,
            notificationsMuted,
            listNumber);
        _cartridge.UpdateCartridgeUiState(loader, state);
    }
    #region Pirate: camera (nanochat gallery)
    private static Dictionary<string, NanoChatPhotoData> BuildUiPhotos(Dictionary<string, NanoChatPhotoData> photos)
    {
        var uiPhotos = new Dictionary<string, NanoChatPhotoData>(photos.Count);
        foreach (var (fileName, photo) in photos)
        {
            uiPhotos[fileName] = new NanoChatPhotoData(
                fileName,
                photo.ImageData,
                photo.PreviewData,
                photo.Caption,
                photo.Description,
                photo.NamesSeen);
        }

        return uiPhotos;
    }

    private static Dictionary<uint, List<NanoChatMessage>> BuildUiMessages(Dictionary<uint, List<NanoChatMessage>> messages)
    {
        var uiMessages = new Dictionary<uint, List<NanoChatMessage>>(messages.Count);
        foreach (var (recipientNumber, messageList) in messages)
        {
            var uiMessageList = new List<NanoChatMessage>(messageList.Count);
            foreach (var message in messageList)
            {
                if (message.Photo is not { } photo)
                {
                    uiMessageList.Add(message);
                    continue;
                }

                var uiPhoto = new NanoChatPhotoData(
                    photo.FileName,
                    photo.ImageData,
                    photo.PreviewData,
                    photo.Caption,
                    photo.Description,
                    photo.NamesSeen);
                uiMessageList.Add(new NanoChatMessage(
                    message.Id,
                    message.Timestamp,
                    message.Content,
                    message.SenderId,
                    message.DeliveryFailed,
                    uiPhoto));
            }

            uiMessages[recipientNumber] = uiMessageList;
        }

        return uiMessages;
    }
    private void ShowPhotoActionSuccess(EntityUid source, EntityUid user, string locId)
    {
        ShowPhotoActionFeedback(source, user, locId, SendSuccessSound);
    }

    private void ShowPhotoActionSuccess(EntityUid source, EntityUid user, string locId, SoundSpecifier sound)
    {
        ShowPhotoActionFeedback(source, user, locId, sound);
    }

    private void ShowPhotoActionError(EntityUid source, EntityUid user, string locId)
    {
        ShowPhotoActionFeedback(source, user, locId, SendErrorSound);
    }

    private void ShowPhotoActionFeedback(EntityUid source, EntityUid user, string locId, SoundSpecifier sound)
    {
        if (Deleted(source) || Deleted(user))
            return;

        _popup.PopupEntity(Loc.GetString(locId), source, user);
        _audio.PlayPvs(sound, source, SenderFeedbackAudioParams);
    }

    private bool TryCreateStoredPhoto(PhotoCardComponent photoCard, out NanoChatPhotoData storedPhoto)
    {
        storedPhoto = default;
        if (photoCard.ImageData is not { Length: > 0 } imageData)
            return false;

        var fileName = GetStoredPhotoFileName(photoCard);
        storedPhoto = new NanoChatPhotoData(
            fileName,
            imageData,
            photoCard.PreviewData is { Length: > 0 } ? photoCard.PreviewData : imageData,
            photoCard.Caption,
            ComposeStoredPhotoDescription(photoCard),
            new List<string>(photoCard.NamesSeen));
        return true;
    }

    private string GetStoredPhotoFileName(PhotoCardComponent photoCard)
    {
        var baseName = SanitizeFileStem(photoCard.CustomName);
        if (string.IsNullOrWhiteSpace(baseName))
            return GenerateTimestampedPhotoFileName();

        if (!baseName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            baseName += ".png";

        return baseName;
    }

    private string GenerateTimestampedPhotoFileName()
    {
        var now = DateTime.Now;
        var stationTime = _timing.CurTime.Subtract(_gameTicker.RoundStartTimeSpan);
        return $"{Content.Shared._Pirate.PirateStationCalendar.CurrentYear % 100:00}{now:MMdd}{stationTime:hhmmss}.png";
    }

    private static string? SanitizeFileStem(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var sanitized = value.Trim();
        foreach (var invalid in Path.GetInvalidFileNameChars())
        {
            sanitized = sanitized.Replace(invalid, '_');
        }

        sanitized = sanitized.Replace('/', '_').Replace('\\', '_');
        return string.IsNullOrWhiteSpace(sanitized) ? null : sanitized;
    }

    private static string? ComposeStoredPhotoDescription(PhotoCardComponent photoCard)
    {
        if (!string.IsNullOrWhiteSpace(photoCard.CustomDescription) &&
            !string.IsNullOrWhiteSpace(photoCard.BaseDescription))
        {
            return $"{photoCard.CustomDescription} - {photoCard.BaseDescription}";
        }

        if (!string.IsNullOrWhiteSpace(photoCard.CustomDescription))
            return photoCard.CustomDescription;

        return string.IsNullOrWhiteSpace(photoCard.BaseDescription) ? null : photoCard.BaseDescription;
    }
    private bool TryStorePhotoFromMessage(Entity<NanoChatCardComponent> card, uint recipientNumber, uint messageId, out string fileName)
    {
        fileName = string.Empty;
        var messages = _nanoChat.GetMessagesForRecipient((card, card.Comp), recipientNumber);
        if (messages == null)
            return false;

        foreach (var message in messages)
        {
            if (message.Id != messageId || message.Photo is not { } photo)
                continue;

            if (!_nanoChat.TryStorePhoto((card, card.Comp), photo))
                return false;

            fileName = photo.FileName;
            return true;
        }

        return false;
    }
    private string BuildNotificationBody(NanoChatMessage message)
    {
        if (message.Photo is not { } photo)
            return Loc.GetString("nano-chat-new-message-body", ("message", TruncateMessage(message.Content)));

        if (!string.IsNullOrWhiteSpace(message.Content))
        {
            return Loc.GetString("nano-chat-new-photo-message-body",
                ("fileName", photo.FileName),
                ("message", TruncateMessage(message.Content)));
        }

        return Loc.GetString("nano-chat-new-photo-message-body-no-text",
            ("fileName", photo.FileName));
    }
    private void OnPdaGetAlternativeVerbs(Entity<PdaComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanInteract ||
            !args.CanAccess ||
            !GetCardEntity(ent.Owner, out _) ||
            !TryGetUploadablePhoto(args.Using, out _, out _))
            return;

        var user = args.User;
        var used = args.Using;
        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("nano-chat-upload-photo-verb"),
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/plus.svg.192dpi.png")),
            Act = () => TryStartPhotoUpload(ent.Owner, user, used),
            Priority = 5
        });
    }

    private void OnPdaPhotoUploadDoAfter(Entity<PdaComponent> ent, ref PdaPhotoUploadDoAfterEvent args)
    {
        if (args.Handled)
        {
            return;
        }

        var cardUid = GetEntity(args.CardUid);
        if (args.Cancelled ||
            cardUid == EntityUid.Invalid ||
            args.Photo.ImageData is not { Length: > 0 } ||
            !TryComp<NanoChatCardComponent>(cardUid, out var cardComp))
        {
            ShowPhotoActionError(ent.Owner, args.User, "nano-chat-photo-upload-failed");
            args.Handled = true;
            return;
        }

        if (!_nanoChat.TryStorePhoto((cardUid, cardComp), args.Photo))
        {
            ShowPhotoActionError(ent.Owner, args.User, "nano-chat-photo-upload-failed");
            args.Handled = true;
            return;
        }

        UpdateUIForCard(cardUid);
        ShowPhotoActionSuccess(cardComp.PdaUid ?? cardUid, args.User, "nano-chat-photo-uploaded", PhotoScanSuccessSound);
        args.Handled = true;
    }

    private void TryStartPhotoUpload(EntityUid pdaUid, EntityUid user, EntityUid? usedUid)
    {
        if (!TryGetUploadablePhoto(usedUid, out var photoUid, out var storedPhoto))
        {
            ShowPhotoActionError(pdaUid, user, "nano-chat-photo-upload-failed");
            return;
        }

        if (!GetCardEntity(pdaUid, out var card))
        {
            ShowPhotoActionError(pdaUid, user, "nano-chat-photo-upload-failed");
            return;
        }

        var doAfter = new DoAfterArgs(EntityManager, user, PhotoUploadDelay, new PdaPhotoUploadDoAfterEvent(GetNetEntity(card.Owner), storedPhoto), pdaUid, target: pdaUid, used: photoUid)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = true
        };

        if (_doAfter.TryStartDoAfter(doAfter))
            return;

        ShowPhotoActionError(pdaUid, user, "nano-chat-photo-upload-failed");
    }

    private bool TryGetUploadablePhoto(EntityUid? usedUid, out EntityUid photoUid, out NanoChatPhotoData storedPhoto)
    {
        photoUid = EntityUid.Invalid;
        storedPhoto = default;
        if (usedUid is not { } uid ||
            !TryComp<PhotoCardComponent>(uid, out var photoCard) ||
            !TryCreateStoredPhoto(photoCard, out storedPhoto))
        {
            return false;
        }

        photoUid = uid;
        return true;
    }
    private void HandleSelectGalleryPhoto(Entity<NanoChatCardComponent> card, NanoChatUiMessageEvent msg)
    {
        if (string.IsNullOrWhiteSpace(msg.PhotoFileName))
        {
            _nanoChat.SetSelectedGalleryPhoto((card, card.Comp), null);
            return;
        }

        _nanoChat.SetSelectedGalleryPhoto(
            (card, card.Comp),
            _nanoChat.TryGetStoredPhoto((card, card.Comp), msg.PhotoFileName, out _)
                ? msg.PhotoFileName
                : null);
    }
    private void HandleDeleteGalleryPhoto(Entity<NanoChatCardComponent> card, NanoChatUiMessageEvent msg)
    {
        if (string.IsNullOrWhiteSpace(msg.PhotoFileName))
            return;

        if (_nanoChat.TryDeleteStoredPhoto((card, card.Comp), msg.PhotoFileName))
        {
            UpdateUIForCard(card);
        }
    }

    private void HandleStoreMessagePhoto(Entity<NanoChatCardComponent> card, NanoChatUiMessageEvent msg)
    {
        if (msg.RecipientNumber == null || msg.MessageId == null)
            return;

        if (!TryStorePhotoFromMessage(card, msg.RecipientNumber.Value, msg.MessageId.Value, out _))
        {
            ShowPhotoActionError(card.Comp.PdaUid ?? card.Owner, msg.Actor, "nano-chat-photo-save-failed");
            return;
        }

        UpdateUIForCard(card);
        ShowPhotoActionSuccess(card.Comp.PdaUid ?? card.Owner, msg.Actor, "nano-chat-photo-saved-to-gallery", PhotoScanSuccessSound); // Pirate: use same confirm sound as scan/print
    }

    private void OnFaxGetAlternativeVerbs(Entity<FaxMachineComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanInteract ||
            !args.CanAccess ||
            !_fax.CanQueueNanoChatPhotoPrint(ent.Owner, ent.Comp) ||
            !TryGetPrintableGalleryPhoto(args.Using, out _, out _))
        {
            return;
        }

        var user = args.User;
        var used = args.Using;
        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("nano-chat-print-photo-verb"),
            Icon = new SpriteSpecifier.Texture(new("/Textures/_Pirate/Interface/VerbIcons/file_public.svg.192dpi.png")),
            Act = () => TryStartPhotoPrintToFax(ent.Owner, user, used),
            Priority = 5
        });
    }

    private void OnFaxPhotoPrintToFaxDoAfter(Entity<FaxMachineComponent> ent, ref PdaPhotoPrintToFaxDoAfterEvent args)
    {
        if (args.Handled)
            return;

        var cardUid = GetEntity(args.CardUid); // Pirate: resolve frozen fax source card from net entity
        if (args.Cancelled ||
            cardUid == EntityUid.Invalid ||
            args.Photo.ImageData is not { Length: > 0 } ||
            !_fax.TryQueueNanoChatPhotoPrint(ent.Owner, args.User, args.Photo, ent.Comp))
        {
            ShowPhotoActionError(ent.Owner, args.User, "nano-chat-photo-print-failed");
            args.Handled = true;
            return;
        }

        ShowPhotoActionSuccess(ent.Owner, args.User, "nano-chat-photo-printed", PhotoScanSuccessSound);
        args.Handled = true;
    }

    private void TryStartPhotoPrintToFax(EntityUid faxUid, EntityUid user, EntityUid? usedUid)
    {
        if (!TryGetPrintableGalleryPhoto(usedUid, out var cardUid, out var storedPhoto) ||
            !TryComp<FaxMachineComponent>(faxUid, out var fax) ||
            !_fax.CanQueueNanoChatPhotoPrint(faxUid, fax))
        {
            ShowPhotoActionError(faxUid, user, "nano-chat-photo-print-failed");
            return;
        }

        var doAfter = new DoAfterArgs(EntityManager, user, PhotoUploadDelay, new PdaPhotoPrintToFaxDoAfterEvent(GetNetEntity(cardUid), storedPhoto), faxUid, target: faxUid, used: usedUid)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = true
        };

        if (_doAfter.TryStartDoAfter(doAfter))
            return;

        ShowPhotoActionError(faxUid, user, "nano-chat-photo-print-failed");
    }

    private bool TryGetPrintableGalleryPhoto(EntityUid? pdaUid, out EntityUid cardUid, out NanoChatPhotoData storedPhoto)
    {
        cardUid = EntityUid.Invalid;
        storedPhoto = default;
        string? selectedPhotoFileName = null;
        if (pdaUid is not { } uid ||
            !GetCardEntity(uid, out var card) ||
            string.IsNullOrWhiteSpace(selectedPhotoFileName = _nanoChat.GetSelectedGalleryPhoto((card, card.Comp))) ||
            !_nanoChat.TryGetStoredPhoto((card, card.Comp), selectedPhotoFileName, out storedPhoto) ||
            storedPhoto.ImageData is not { Length: > 0 })
        {
            return false;
        }

        cardUid = card.Owner;
        return true;
    }
    #endregion

}
