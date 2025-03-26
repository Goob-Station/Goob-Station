using System.Linq;

namespace Content.Server._Goobstation.Chat;

public sealed class ChatTriggerSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChatTriggerComponent, BeforeChatMessageSentEvent>(OnBeforeMessageSent);
    }

    private void OnBeforeMessageSent(Entity<ChatTriggerComponent> ent, ref BeforeChatMessageSentEvent args)
    {
        if (args.Cancelled)
            return;

        var sentMessage = args.Message;
        var desiredIcType = args.ICChatType;
        var desiredOocType = args.OOCChatType;

        if (string.IsNullOrEmpty(sentMessage))
            return;

        var msg = string.Empty;
        var chatTriggerData = ent.Comp.MessageData.FirstOrDefault(CheckTriggerData);
        if (chatTriggerData == null)
            return;

        args.ShouldSanitize &= chatTriggerData.ShouldSanitize;
        args.HasRadioPrefix &= chatTriggerData.ShouldSendRadio;

        var ev = chatTriggerData.Event;
        ev.Performer = ent;
        ev.Message = msg;

        RaiseLocalEvent(ent, ev, true);

        return;

        bool CheckTriggerData(ChatTriggerData data)
        {
            if ((desiredIcType == null || !data.DesiredICMessageTypes.Contains(desiredIcType.Value)) &&
                (desiredOocType == null || !data.DesiredOOCMessageTypes.Contains(desiredOocType.Value)))
                return false;

            foreach (var (locId, messageCheckData) in data.AllowedMessages)
            {
                if (!CheckForMessage(locId, messageCheckData))
                    continue;

                return true;
            }

            return false;
        }

        bool CheckForMessage(LocId locId, MessageCheckData messageCheckData)
        {
            msg = Loc.GetString(locId);

            var comparison = messageCheckData.IgnoreCase
                ? StringComparison.InvariantCultureIgnoreCase
                : StringComparison.InvariantCulture;

            return messageCheckData.CheckFullMessage
                ? sentMessage.Equals(msg, comparison)
                : sentMessage.Contains(msg, comparison);
        }
    }
}
