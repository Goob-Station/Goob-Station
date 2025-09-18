using System.Linq;
using Content.Server._EinsteinEngines.Language;
using Content.Server.Administration.Managers;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.Language;
using Content.Shared._EinsteinEngines.Language;
using Content.Shared.Chat;
using Content.Shared.Language;
using Content.Shared.WhiteDream.BloodCult.BloodCultist;
using Content.Shared.WhiteDream.BloodCult.Constructs;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Server.WhiteDream.BloodCult;

public sealed class BloodCultChatSystem : EntitySystem
{
    [Dependency] private readonly IAdminManager _adminManager = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;

    [Dependency] private readonly LanguageSystem _language = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<Goobstation.Shared.BloodCult.BloodCultist.BloodCultistComponent, EntitySpokeEvent>(OnCultistSpeak);
        SubscribeLocalEvent<Goobstation.Shared.BloodCult.Constructs.ConstructComponent, EntitySpokeEvent>(OnConstructSpeak);
    }

    private void OnCultistSpeak(EntityUid uid, Goobstation.Shared.BloodCult.BloodCultist.BloodCultistComponent component, EntitySpokeEvent args)
    {
        if (args.Source != uid || args.Language.ID != component.CultLanguageId || args.IsWhisper)
            return;

        SendMessage(args.Source, args.Message, false, args.Language);
    }

    private void OnConstructSpeak(EntityUid uid, Goobstation.Shared.BloodCult.Constructs.ConstructComponent component, EntitySpokeEvent args)
    {
        if (args.Source != uid || args.Language.ID != component.CultLanguageId || args.IsWhisper)
            return;

        SendMessage(args.Source, args.Message, false, args.Language);
    }

    private void SendMessage(EntityUid source, string message, bool hideChat, LanguagePrototype language)
    {
        var clients = GetClients(language.ID);
        var playerName = Name(source);
        var wrappedMessage = Loc.GetString("chat-manager-send-cult-chat-wrap-message",
            ("channelName", Loc.GetString("chat-manager-cult-channel-name")),
            ("player", playerName),
            ("message", FormattedMessage.EscapeText(message)));

        _chatManager.ChatMessageToMany(ChatChannel.Telepathic,
            message,
            wrappedMessage,
            source,
            hideChat,
            true,
            clients.ToList(),
            language.SpeechOverride.Color);
    }

    private IEnumerable<INetChannel> GetClients(string languageId)
    {
        return Filter.Empty()
            .AddWhereAttachedEntity(entity => CanHearBloodCult(entity, languageId))
            .Recipients
            .Union(_adminManager.ActiveAdmins)
            .Select(p => p.Channel);
    }

    private bool CanHearBloodCult(EntityUid entity, string languageId)
    {
        var understood = _language.GetUnderstoodLanguages(entity);
        return understood.Any(language => language.Id == languageId);
    }
}
