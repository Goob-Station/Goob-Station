using System.Linq;
using Content.Server._EinsteinEngines.Language;
using Content.Server.Administration.Managers;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Shared._EinsteinEngines.Language;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server._White.Xenomorphs.Xenomorph;

public sealed class XenomorphSystem : SharedXenomorphSystem
{
    [Dependency] private readonly IAdminManager _adminManager = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly LanguageSystem _language = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XenomorphComponent, EntitySpokeEvent>(OnEntitySpoke);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var time = _timing.CurTime;

        var query = EntityQueryEnumerator<XenomorphComponent>();
        while (query.MoveNext(out var uid, out var xenomorph))
        {
            if (xenomorph.WeedHeal == null || !xenomorph.OnWeed || time < xenomorph.NextPointsAt)
                continue;

            xenomorph.NextPointsAt = time + xenomorph.WeedHealRate;

            _damageable.TryChangeDamage(uid, xenomorph.WeedHeal);
        }
    }

    private void OnEntitySpoke(EntityUid uid, XenomorphComponent component, EntitySpokeEvent args)
    {
        if (args.Source != uid || args.Language.ID != component.XenoLanguageId || args.IsWhisper)
            return;

        SendMessage(args.Source, args.Message, false, args.Language);
    }

    private bool CanHearXenoHivemind(EntityUid entity, string languageId)
    {
        var understood = _language.GetUnderstoodLanguages(entity);
        return understood.Any(language => language.Id == languageId);
    }

    private void SendMessage(EntityUid source, string message, bool hideChat, LanguagePrototype language)
    {
        var clients = GetClients(language.ID);
        var playerName = Name(source);
        var wrappedMessage = Loc.GetString(
            "chat-manager-send-xeno-hivemind-chat-wrap-message",
            ("channelName", Loc.GetString("chat-manager-xeno-hivemind-channel-name")),
            ("player", playerName),
            ("message", FormattedMessage.EscapeText(message)));

        _chatManager.ChatMessageToMany(
            ChatChannel.Telepathic,
            message,
            wrappedMessage,
            source,
            hideChat,
            true,
            clients.ToList(),
            language.SpeechOverride.Color);
    }

    private IEnumerable<INetChannel> GetClients(string languageId) =>
        Filter.Empty()
            .AddWhereAttachedEntity(entity => CanHearXenoHivemind(entity, languageId))
            .Recipients
            .Union(_adminManager.ActiveAdmins)
            .Select(p => p.Channel);
}
