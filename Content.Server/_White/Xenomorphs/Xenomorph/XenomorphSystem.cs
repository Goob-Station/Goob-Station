using System.Linq;
using Content.Goobstation.Maths.FixedPoint;
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
    [Dependency] private readonly WoundSystem _wounds = default!; // Goobstation
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!; // Goobstation
    [Dependency] private readonly BodySystem _body = default!; // Goobstation

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XenomorphComponent, EntitySpokeEvent>(OnEntitySpoke);
    }

public override void Update(float frameTime)
{
    // Goobstation start
    base.Update(frameTime);

    var time = _timing.CurTime;
    var query = EntityQueryEnumerator<XenomorphComponent, BloodstreamComponent, BodyComponent>();  // Added BodyComponent to query

    while (query.MoveNext(out var uid, out var xenomorph, out var bloodstream, out var body))
    {
        if (xenomorph.WeedHeal == null || time < xenomorph.NextPointsAt)
            continue;

        // Update next heal time
        xenomorph.NextPointsAt = time + xenomorph.WeedHealRate;

        if (!xenomorph.OnWeed)
            continue;

        // Apply regular weed healing if on weeds
        _damageable.TryChangeDamage(uid, xenomorph.WeedHeal);

        // Process bleeding and blood loss in parallel with cached values
        ProcessBleeding(uid, body);
        ProcessBloodLoss(uid, bloodstream);
    }
}

// Heal/Seal any bleeding parts over time.
private void ProcessBleeding(EntityUid uid, BodyComponent body)
{
    const float bleedReduction = 0.5f;
    var reduction = FixedPoint2.New(bleedReduction);

    // Get Bodyparts
    var bodyParts = _body.GetBodyChildren(uid, body).ToList();

    foreach (var part in bodyParts)
    {
        // Process all wounds in this part
        foreach (var wound in _wounds.GetWoundableWounds(part.Id))
        {
            if (!TryComp<BleedInflicterComponent>(wound, out var bleedComp) ||
                !bleedComp.IsBleeding ||
                bleedComp.BleedingAmountRaw <= FixedPoint2.Zero)
            {
                continue;
            }

            // Calculate new bleed amount
            var newBleed = FixedPoint2.Max(FixedPoint2.Zero, bleedComp.BleedingAmountRaw - reduction);
            var amountHealed = bleedComp.BleedingAmountRaw - newBleed;

            if (amountHealed <= FixedPoint2.Zero)
                continue;

            // Apply changes
            bleedComp.BleedingAmountRaw = newBleed;
            Dirty(wound, bleedComp);
        }
    }
}

// Slowly heal bloodloss
private void ProcessBloodLoss(EntityUid uid, BloodstreamComponent bloodstream)
{
    if (!_solutionContainer.ResolveSolution(uid,
            bloodstream.BloodSolutionName,
            ref bloodstream.BloodSolution,
            out var bloodSolution)
            || bloodSolution.Volume >= bloodstream.BloodMaxVolume)
    {
        return;
    }

    var bloodloss = new DamageSpecifier();
    bloodloss.DamageDict["Bloodloss"] = -0.2f;  // Heal blood per tick
    _damageable.TryChangeDamage(uid, bloodloss);
}
// Goobstation end

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
