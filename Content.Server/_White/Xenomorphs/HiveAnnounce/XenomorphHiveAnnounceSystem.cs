using System.IO;
using Content.Goobstation.Shared.Xenomorph;
using Content.Server.Administration;
using Content.Server.Chat.Managers;
using Content.Shared.Actions;
using Content.Shared.Body.Systems;
using Content.Shared.Chat;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared._White.Xenomorphs.HiveAnnounce;
using Content.Shared._White.Xenomorphs.Ovipositor;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Server._White.Xenomorphs.HiveAnnounce;

public sealed class XenomorphHiveAnnounceSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly QuickDialogSystem _quickDialog = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XenomorphHiveAnnounceComponent, ComponentStartup>(OnAnnounceStartup);
        SubscribeLocalEvent<XenomorphHiveAnnounceComponent, ComponentShutdown>(OnAnnounceShutdown);
        SubscribeLocalEvent<XenomorphHiveAnnounceComponent, XenomorphHiveAnnounceActionEvent>(OnAnnounce);
    }

    private void OnAnnounceStartup(EntityUid uid, XenomorphHiveAnnounceComponent component, ComponentStartup args)
    {
        if (component.ActionEntity == null)
            _actions.AddAction(uid, ref component.ActionEntity, component.ActionId);
    }

    private void OnAnnounceShutdown(EntityUid uid, XenomorphHiveAnnounceComponent component, ComponentShutdown args) =>
        _actions.RemoveAction(uid, component.ActionEntity);

    private void OnAnnounce(EntityUid uid, XenomorphHiveAnnounceComponent component, XenomorphHiveAnnounceActionEvent args)
    {
        if (args.Handled || !_mobState.IsAlive(uid) || !TryComp(uid, out ActorComponent? actor))
            return;

        if (!HasComp<XenomorphOvipositorComponent>(uid))
            return;

        args.Handled = true;

        _quickDialog.OpenDialog(actor.PlayerSession,
            Loc.GetString("xenomorphs-hive-announce-dialog-title"),
            Loc.GetString("xenomorphs-hive-announce-dialog-prompt"),
            (string message) =>
            {
                if (Deleted(uid) || !_mobState.IsAlive(uid) || actor.PlayerSession.AttachedEntity != uid)
                    return;

                if (!HasComp<XenomorphOvipositorComponent>(uid))
                    return;

                message = message.Trim();
                if (string.IsNullOrWhiteSpace(message))
                {
                    _popup.PopupEntity(Loc.GetString("xenomorphs-hive-announce-empty"), uid, uid);
                    return;
                }

                if (message.Length > component.MaxLength)
                    message = message[..component.MaxLength];

                SendHiveAnnounce(uid, component, message);
            });
    }

    private void SendHiveAnnounce(EntityUid source, XenomorphHiveAnnounceComponent component, string message)
    {
        var filter = Filter.Empty();
        var query = EntityQueryEnumerator<ActorComponent>();
        while (query.MoveNext(out var uid, out var actor))
        {
            if (!_mobState.IsAlive(uid))
                continue;

            if (!_body.TryGetBodyOrganEntityComps<HiveNodeComponent>(uid, out _))
                continue;

            filter.AddPlayer(actor.PlayerSession);
        }

        if (filter.Count == 0)
            return;

        var escaped = FormattedMessage.EscapeText(message);
        var wrapped = Loc.TryGetString(
            "xenomorphs-hive-announce-wrap",
            out var localized,
            ("message", escaped))
            ? localized
            : $"a screech in your head\n[italic]{escaped}[/italic]";

        _chat.ChatMessageToManyFiltered(
            filter,
            ChatChannel.Telepathic,
            message,
            wrapped,
            source,
            false,
            true,
            Color.FromHex("#7CFC00"));

        if (component.Sound is { } sound)
        {
            try
            {
                _audio.PlayGlobal(sound, filter, true);
            }
            catch (FileNotFoundException)
            {
                // Missing announce audio must not kill the server.
            }
        }

        _popup.PopupEntity(Loc.GetString("xenomorphs-hive-announce-sent"), source, source);
    }
}
