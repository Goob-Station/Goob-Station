using System.Linq;
using Content.Server._EinsteinEngines.Language;
using Content.Server.Administration.Managers;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Shared._EinsteinEngines.Language;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.Body.Components;
using Content.Shared.Chat;
using Content.Shared.Chemistry.EntitySystems;
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
    [Dependency] private readonly WoundSystem _wounds = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly BodySystem _body = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XenomorphComponent, EntitySpokeEvent>(OnEntitySpoke);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var time = _timing.CurTime;
        var query = EntityQueryEnumerator<XenomorphComponent, BloodstreamComponent>();

        while (query.MoveNext(out var uid, out var xenomorph, out var bloodstream))
        {
            if (xenomorph.WeedHeal == null || time < xenomorph.NextPointsAt)
                continue;

            // Update next heal time
            xenomorph.NextPointsAt = time + xenomorph.WeedHealRate;

            if (!xenomorph.OnWeed)
                continue;

            // Apply regular weed healing if on weeds
            _damageable.TryChangeDamage(uid, xenomorph.WeedHeal);

            // Then ensure all wounds are marked as not bleeding
            if (TryComp<BodyComponent>(uid, out var body))
            {
                // Get all body parts
                foreach (var part in _body.GetBodyChildren(uid, body))
                {
                    // Get all wounds on this body part
                    var wounds = _wounds.GetWoundableWounds(part.Id);
                    foreach (var wound in wounds)
                    {
                        //  Remove & Add the bleeding component to get rid of bleeding & allow it to work
                        if (!HasComp<BleedInflicterComponent>(wound))
                            continue;
                        RemComp<BleedInflicterComponent>(wound);
                        AddComp<BleedInflicterComponent>(wound);
                        Log.Debug($"Stopped bleeding on wound {ToPrettyString(wound)}");
                    }
                }
            }

            // Heal existing blood loss damage if not at max blood
            if (_solutionContainer.ResolveSolution(uid, bloodstream.BloodSolutionName, ref bloodstream.BloodSolution, out var bloodSolution)
                && bloodSolution.Volume < bloodstream.BloodMaxVolume)
            {
                var bloodloss = new DamageSpecifier();
                bloodloss.DamageDict.Add("Bloodloss", -0.2); // Heal blood per tick.
                _damageable.TryChangeDamage(uid, bloodloss);
                Log.Debug("Healing blood loss");
            }
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
