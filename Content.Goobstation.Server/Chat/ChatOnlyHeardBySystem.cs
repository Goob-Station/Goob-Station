using Content.Goobstation.Common.Chat;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Shared.Chat;
using Content.Shared.Ghost;
using Robust.Server.Player;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Chat;

/// <summary>
/// Reroutes the IC chat of entities with ChatOnlyHeardByComponent
/// so only the speaker and their designated listener receive it.
/// </summary>
public sealed class ChatOnlyHeardBySystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChatOnlyHeardByComponent, BeforeChatMessageSentEvent>(OnBeforeChatSent);
    }

    private void OnBeforeChatSent(Entity<ChatOnlyHeardByComponent> ent, ref BeforeChatMessageSentEvent args)
    {
        args.Cancelled = true;

        var channel = (InGameICChatType) args.Channel;
        var name = ent.Comp.NameColor is { } color
            ? $"[color={color.ToHex()}]{Name(ent)}[/color]"
            : Name(ent);

        var wrapped = channel switch
        {
            InGameICChatType.Speak => _chat.WrapPublicMessage(ent, name, args.Message),
            InGameICChatType.Whisper => _chat.WrapWhisperMessage(ent,
                "chat-manager-entity-whisper-wrap-message",
                name,
                args.Message),
            InGameICChatType.Emote => Loc.GetString("chat-manager-entity-me-wrap-message",
                ("entityName", Name(ent)),
                ("entity", ent.Owner),
                ("message", FormattedMessage.RemoveMarkupOrThrow(args.Message))),
            _ => null,
        };

        if (wrapped == null)
            return;

        var chatChannel = channel switch
        {
            InGameICChatType.Whisper => ChatChannel.Whisper,
            InGameICChatType.Emote => ChatChannel.Emotes,
            _ => ChatChannel.Local,
        };

        if (_player.TryGetSessionByEntity(ent, out var ownSession))
            _chatManager.ChatMessageToOne(chatChannel, args.Message, wrapped, ent, false, ownSession.Channel);

        foreach (var session in _player.Sessions)
        {
            if (session.AttachedEntity is not { } attached
                || attached == ent.Owner
                || attached == ent.Comp.Listener
                || !HasComp<GhostHearingComponent>(attached))
                continue;

            _chatManager.ChatMessageToOne(chatChannel, args.Message, wrapped, ent, false, session.Channel);
        }

        if (ent.Comp.Listener is not { } listener || TerminatingOrDeleted(listener))
            return;

        var range = channel == InGameICChatType.Whisper
            ? SharedChatSystem.WhisperMuffledRange
            : SharedChatSystem.VoiceRange;

        if (Transform(ent).Coordinates.TryDistance(EntityManager, Transform(listener).Coordinates, out var distance)
            && distance <= range
            && _player.TryGetSessionByEntity(listener, out var listenerSession))
            _chatManager.ChatMessageToOne(chatChannel, args.Message, wrapped, ent, false, listenerSession.Channel);
    }
}
