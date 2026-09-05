using Content.Shared._DV.CartridgeLoader.Cartridges;

using Content.Shared._DV.NanoChat;

namespace Content.Server._DV.CartridgeLoader.Cartridges;

public sealed partial class NanoChatCartridgeSystem : EntitySystem // Allll larp
{

    /// <summary>
    /// Delivers a message from an anonymous (numberless) sender directly to a recipient's card,
    /// Use this when there is no real sender card.
    /// </summary>
    public void DeliverAnonymousMessage(
        Entity<NanoChatCardComponent> recipient,
        uint senderNumber,
        string senderName,
        string content)
    {
        var message = new NanoChatMessage(_timing.CurTime, content, senderNumber);

        _nanoChat.SetRecipient((recipient, recipient.Comp), senderNumber,
            new NanoChatRecipient(senderNumber, senderName));

        _nanoChat.AddMessage((recipient, recipient.Comp), senderNumber, message);

        HandleUnreadNotification(recipient, message, senderNumber);

        var msgEv = new NanoChatMessageReceivedEvent(recipient);
        RaiseLocalEvent(ref msgEv);
        UpdateUIForCard(recipient);
    }
}
