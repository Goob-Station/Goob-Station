using Content.Server.Chat.Systems;
using Content.Shared.Magic;
using Content.Shared.Magic.Events;

namespace Content.Server.Magic;

public sealed class MagicSystem : SharedMagicSystem
{
    [Dependency] private readonly ChatSystem _chat = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpeakSpellEvent>(OnSpellSpoken);
    }

    private void OnSpellSpoken(ref SpeakSpellEvent args)
    {
        var chatType = (Content.Server.Chat.Systems.InGameICChatType) ((int) args.ChatType);
        _chat.TrySendInGameICMessage(args.Performer, Loc.GetString(args.Speech), chatType, false);
    }
}
