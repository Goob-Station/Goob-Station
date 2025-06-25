using System.Linq;
using Content.Server.Administration.Managers;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.Language;
using Content.Shared._EE.Shadowling;
using Content.Shared.Chat;
using Content.Shared.Language;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Utility;


namespace Content.Server._EE.Shadowling;


/// <summary>
/// This handles Shadowling/Thrall communication
/// </summary>
public sealed class ShadowlingChatSystem : EntitySystem
{
    [Dependency] private readonly IAdminManager _adminManager = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;

    [Dependency] private readonly LanguageSystem _language = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingComponent, EntitySpokeEvent>(OnShadowlingSpeak);
        SubscribeLocalEvent<ThrallComponent, EntitySpokeEvent>(OnThrallSpeak);
    }

    private void OnShadowlingSpeak(EntityUid uid, ShadowlingComponent component, EntitySpokeEvent args)
    {
        if (args.Source != uid || args.Language.ID != component.SlingLanguageId || args.IsWhisper)
            return;

        SendMessage(args.Source, args.Message, false, args.Language);
    }
    private void OnThrallSpeak(EntityUid uid, ThrallComponent component, EntitySpokeEvent args)
    {
        if (args.Source != uid || args.Language.ID != component.SlingLanguageId || args.IsWhisper)
            return;

        SendMessage(args.Source, args.Message, false, args.Language);
    }

    private void SendMessage(EntityUid source, string message, bool hideChat, LanguagePrototype language)
    {
        // Again, this is thanks to blood cult (I didn't wanna touch their code and break stuff so I just copypasted it)
        var clients = GetClients(language.ID);
        var playerName = Name(source);
        var wrappedMessage = Loc.GetString("chat-manager-send-cult-chat-wrap-message",
            ("channelName", Loc.GetString("chat-manager-shadowling-channel-name")),
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
            .AddWhereAttachedEntity(entity => CanHear(entity, languageId))
            .Recipients
            .Union(_adminManager.ActiveAdmins)
            .Select(p => p.Channel);
    }

    private bool CanHear(EntityUid entity, string languageId)
    {
        var understood = _language.GetUnderstoodLanguages(entity);
        return understood.Any(language => language.Id == languageId);
    }
}
