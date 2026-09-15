// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration;
using Content.Shared.IdentityManagement;
using Content.Server.Popups;
using Content.Server.Prayer;
using Robust.Shared.Player;
using Robust.Shared.Utility;
using Content.Goobstation.Shared.Telepathy;
using Content.Shared.Actions;
using Content.Shared.Whitelist;
using Content.Shared.Telepathy;
using Content.Shared.Popups;
using Content.Server.Chat.Managers;
using Content.Shared.Chat;
using Content.Server.Administration.Managers;
using Content.Shared.Database;
using Content.Server.Administration.Logs;

namespace Content.Goobstation.Server.Telepathy;

/// <summary>
/// This handles the Demonic Whisper logic.
/// Demonic Whisper lets you send a subtle popup to someone.
/// </summary>
public sealed class TelepathySystem : SharedTelepathySystem
{
    [Dependency] private readonly QuickDialogSystem _quickDialog = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IdentitySystem _identity = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IAdminLogManager _adminLog = default!;

    private EntityQuery<ActorComponent> _actorQuery;

    public override void Initialize()
    {
        base.Initialize();

        _actorQuery = GetEntityQuery<ActorComponent>();

        SubscribeLocalEvent<TelepathyWhisperEvent>(OnTelepathyWhisper);
    }

    private void OnTelepathyWhisper(TelepathyWhisperEvent args)
    {
        EntityUid performer = args.Performer;
        EntityUid target = args.Target;

        if (!TryComp(args.Action, out TelepathyActionComponent? telepathy)
            || !_whitelist.IsWhitelistPassOrNull(telepathy.TargetWhitelist, target)
            || !_actorQuery.TryComp(performer, out ActorComponent? actor)
            || !_actorQuery.TryComp(target, out ActorComponent? actorTarget))
            return;

        _quickDialog.OpenDialog(actor.PlayerSession, Loc.GetString(telepathy.DialogueTitle), "Message", (string message) =>
        {
            // Suddenly, a voice resonates in your head...
            // blah blah
            if (telepathy.PopupWhisperFlavor is not null)
                _chatManager.ChatMessageToOne(
                    ChatChannel.Local,
                    "",
                    Loc.GetString(telepathy.PopupWhisperFlavor),
                    EntityUid.Invalid,
                    false,
                    actorTarget.PlayerSession.Channel
                );
            _popup.PopupEntity(message, target, target, telepathy.PopupType);

            // You whisper to
            _popup.PopupEntity(Loc.GetString(telepathy.PopupWhisperSelf,
                ("name", _identity.GetEntityIdentity(target)),
                ("message", FormattedMessage.EscapeText(message))),
                performer, performer);

            _adminLog.Add(LogType.AdminMessage, LogImpact.Low, $"{ToPrettyString(target):player} received telepathic message from {ToPrettyString(performer):player}: {message}");
        });
    }
}
