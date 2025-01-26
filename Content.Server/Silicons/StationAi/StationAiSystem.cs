using System.Linq;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Shared.Chat;
using Content.Shared.Mind;
using Content.Shared.Roles;
using Content.Shared.Silicons.StationAi;
using Content.Shared.StationAi;
using Robust.Shared.Audio;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
using static Content.Server.Chat.Systems.ChatSystem;
using Content.Server.Administration; // Goobstation - borg order start
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Mind.Components;
using Content.Shared.Silicons.Laws.Components;
using Content.Server.Silicons.Laws;
using Content.Shared.Popups; // Goobstation - end

namespace Content.Server.Silicons.StationAi;

public sealed class StationAiSystem : SharedStationAiSystem
{
    [Dependency] private readonly IChatManager _chats = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedTransformSystem _xforms = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedRoleSystem _roles = default!;
    [Dependency] private readonly QuickDialogSystem _quickDialog = default!; // Goobstation - Borg announce start
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SiliconLawSystem _law = default!; // Goobstation - end

    private readonly HashSet<Entity<StationAiCoreComponent>> _ais = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExpandICChatRecipientsEvent>(OnExpandICChatRecipients);
        SubscribeLocalEvent<StationAiHeldComponent, SiliconAnnounceEvent>(OnBorgAnnounce); // Goobstation - Borg announce
    }

    private void OnExpandICChatRecipients(ExpandICChatRecipientsEvent ev)
    {
        var xformQuery = GetEntityQuery<TransformComponent>();
        var sourceXform = Transform(ev.Source);
        var sourcePos = _xforms.GetWorldPosition(sourceXform, xformQuery);

        // This function ensures that chat popups appear on camera views that have connected microphones.
        var query = EntityManager.EntityQueryEnumerator<StationAiCoreComponent, TransformComponent>();
        while (query.MoveNext(out var ent, out var entStationAiCore, out var entXform))
        {
            var stationAiCore = new Entity<StationAiCoreComponent>(ent, entStationAiCore);

            if (!TryGetInsertedAI(stationAiCore, out var insertedAi) || !TryComp(insertedAi, out ActorComponent? actor))
                return;

            if (stationAiCore.Comp.RemoteEntity == null || stationAiCore.Comp.Remote)
                return;

            var xform = Transform(stationAiCore.Comp.RemoteEntity.Value);

            var range = (xform.MapID != sourceXform.MapID)
                ? -1
                : (sourcePos - _xforms.GetWorldPosition(xform, xformQuery)).Length();

            if (range < 0 || range > ev.VoiceRange)
                continue;

            ev.Recipients.TryAdd(actor.PlayerSession, new ICChatRecipientData(range, false));
        }
    }

    public override bool SetVisionEnabled(Entity<StationAiVisionComponent> entity, bool enabled, bool announce = false)
    {
        if (!base.SetVisionEnabled(entity, enabled, announce))
            return false;

        if (announce)
        {
            AnnounceSnip(entity.Owner);
        }

        return true;
    }

    public override bool SetWhitelistEnabled(Entity<StationAiWhitelistComponent> entity, bool enabled, bool announce = false)
    {
        if (!base.SetWhitelistEnabled(entity, enabled, announce))
            return false;

        if (announce)
        {
            AnnounceSnip(entity.Owner);
        }

        return true;
    }

    public override void AnnounceIntellicardUsage(EntityUid uid, SoundSpecifier? cue = null)
    {
        if (!TryComp<ActorComponent>(uid, out var actor))
            return;

        var msg = Loc.GetString("ai-consciousness-download-warning");
        var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", msg));
        _chats.ChatMessageToOne(ChatChannel.Server, msg, wrappedMessage, default, false, actor.PlayerSession.Channel, colorOverride: Color.Red);

        if (cue != null && _mind.TryGetMind(uid, out var mindId, out _))
            _roles.MindPlaySound(mindId, cue);
    }

    private void AnnounceSnip(EntityUid entity)
    {
        var xform = Transform(entity);

        if (!TryComp(xform.GridUid, out MapGridComponent? grid))
            return;

        _ais.Clear();
        _lookup.GetChildEntities(xform.GridUid.Value, _ais);
        var filter = Filter.Empty();

        foreach (var ai in _ais)
        {
            // TODO: Filter API?
            if (TryComp(ai.Owner, out ActorComponent? actorComp))
            {
                filter.AddPlayer(actorComp.PlayerSession);
            }
        }

        // TEST
        // filter = Filter.Broadcast();

        // No easy way to do chat notif embeds atm.
        var tile = Maps.LocalToTile(xform.GridUid.Value, grid, xform.Coordinates);
        var msg = Loc.GetString("ai-wire-snipped", ("coords", tile));

        _chats.ChatMessageToMany(ChatChannel.Notifications, msg, msg, entity, false, true, filter.Recipients.Select(o => o.Channel));
        // Apparently there's no sound for this.
    }

    // Goobstation - Borg announce
    private void OnBorgAnnounce(Entity<StationAiHeldComponent> ent, ref SiliconAnnounceEvent args)
    {
        if (!TryComp<ActorComponent>(ent.Owner, out var actor))
            return;

        _quickDialog.OpenDialog(actor.PlayerSession, Loc.GetString("ai-borg-order-prompt-tittle"), "", (string result) =>
        {
            if (result.Length <= 0)
                return;
            if (result.Length >= 155)
                result = result.Substring(0, 155) + "...";

            SendBorgOrder(result);
        });
    }

    /// <summary>
    /// Send a order popup to all borgs that have ai law
    /// </summary>
    public void SendBorgOrder(string order, PopupType popupType = PopupType.Small)
    {
        var message = Loc.GetString("ai-borg-order-popup-text", ("message", order));
        var borg = AllEntityQuery<BorgChassisComponent, SiliconLawBoundComponent, MindContainerComponent>();
        while (borg.MoveNext(out var uid, out _, out var slb, out var mc))
        {
            if (HasComp<BorgChassisComponent>(uid) ||
                HasComp<MindContainerComponent>(uid) ||
                HasComp<SiliconLawBoundComponent>(uid))
                continue;

            var laws = _law.GetLaws(uid, slb).Laws;
            if (_law.HasLawLocale(laws, "law-obeyai"))
                continue; // theres no way to verify a law prototype in this shit

            _popup.PopupEntity(message, uid, popupType);
        }
    }
}
