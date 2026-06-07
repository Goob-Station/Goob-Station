// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.SlaughterDemon;
using Content.Server.Administration;
using Content.Server.IdentityManagement;
using Content.Server.Popups;
using Content.Server.Prayer;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.SlaughterDemon;

/// <summary>
/// This handles the Demonic Whisper logic.
/// Demonic Whisper lets you send a subtle popup to someone.
/// </summary>
public sealed class DemonicWhisperSystem : EntitySystem
{
    [Dependency] private readonly QuickDialogSystem _quickDialog = default!;
    [Dependency] private readonly PrayerSystem _prayer = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IdentitySystem _identity = default!;

    private EntityQuery<ActorComponent> _actorQuery;

    public override void Initialize()
    {
        base.Initialize();

        _actorQuery = GetEntityQuery<ActorComponent>();

        SubscribeLocalEvent<DemonicWhisperComponent, DemonicWhisperEvent>(OnDemonicWhisper);
    }

    private void OnDemonicWhisper(Entity<DemonicWhisperComponent> ent, ref DemonicWhisperEvent args)
    {
        var target = args.Target;

        if (!_actorQuery.TryComp(ent.Owner, out var actor)
            || !_actorQuery.TryComp(target, out var actorTarget))
            return;

        _quickDialog.OpenDialog(actor.PlayerSession, Loc.GetString("demonic-whisper-title"), "Message", (string message) =>
        {
            _prayer.SendSubtleMessage(actorTarget.PlayerSession, actor.PlayerSession, message, Loc.GetString("demonic-whisper-popup"));

            _popup.PopupEntity(Loc.GetString("demonic-whisper-whisper",
                ("name", _identity.GetEntityIdentity(target)),
                ("message", FormattedMessage.EscapeText(message))),
                ent.Owner);
        });
    }
}
