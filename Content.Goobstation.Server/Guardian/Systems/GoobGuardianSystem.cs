// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Guardian.Components;
using Content.Goobstation.Server.Guardian.Event;
using Content.Goobstation.Shared.Guardian;
using Content.Server.Actions;
using Content.Server.Administration;
using Content.Server.Guardian;
using Content.Server.Popups;
using Content.Server.Prayer;
using Content.Shared.Popups;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Guardian.Systems;
public sealed class GoobGuardianSystem : EntitySystem
{
    [Dependency] private readonly GuardianSystem _guardian = default!;
    [Dependency] private readonly ActionsSystem _action = default!;
    [Dependency] private readonly QuickDialogSystem _dialog = default!;
    [Dependency] private readonly SharedPopupSystem _popups = default!;
    [Dependency] private readonly PrayerSystem _prayer = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GuardianWhisperComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<GuardianWhisperComponent, ComponentShutdown>(OnComponentShutdown);
        SubscribeLocalEvent<GuardianComponent, WhisperGuardianEvent>(OnWhisperGuardian);
        SubscribeLocalEvent<GuardianComponent, GuardianToggleSelfActionEvent>(OnPerformSelfAction);
    }

    private void OnComponentStartup(Entity<GuardianWhisperComponent> ent, ref ComponentStartup args)
    {
        _action.AddAction(ent.Owner, ref ent.Comp.ActionUid, ent.Comp.GuardianWhisper);
    }

    private void OnComponentShutdown(Entity<GuardianWhisperComponent> ent, ref ComponentShutdown args)
    {
        _action.RemoveAction(ent.Owner, ent.Comp.ActionUid);
    }

    private void OnWhisperGuardian(Entity<GuardianComponent> ent, ref WhisperGuardianEvent args)
    {
        if (!HasComp<GuardianWhisperComponent>(ent.Owner))
            return;

        if (!TryComp<ActorComponent>(ent.Owner, out var guardianActor)
        || !TryComp<ActorComponent>(ent.Comp.Host, out var hostActor))
            return;

        _dialog.OpenDialog(guardianActor.PlayerSession, Loc.GetString("guardian-whisper-title"), Loc.GetString("guardian-whisper-prompt"), (string message) =>
        {
            _prayer.SendSubtleMessage(hostActor.PlayerSession, guardianActor.PlayerSession, message, Loc.GetString("guardian-whisper-popup", ("guardian", ent.Owner)));
            _popups.PopupEntity(Loc.GetString("guardian-whisper-whisper"), ent.Owner, ent.Owner);
        });
    }

    private void OnPerformSelfAction(Entity<GuardianComponent> ent, ref GuardianToggleSelfActionEvent args)
    {
        if (ent.Comp.Host != null && TryComp<GuardianHostComponent>(ent.Comp.Host, out var hostComp) && ent.Comp.GuardianLoose)
            _guardian.ToggleGuardian(ent.Comp.Host.Value, hostComp);

        args.Handled = true;
    }
}
