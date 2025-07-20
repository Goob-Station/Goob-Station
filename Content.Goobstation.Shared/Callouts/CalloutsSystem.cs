// SPDX-FileCopyrightText: 2025 ThunderBear2006 <bearthunder06@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Content.Shared.Chat;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.Callouts;

public sealed class CalloutsSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedChatSystem _chat = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CalloutComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<CalloutComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<CalloutComponent, CalloutActionEvent>(OnCallout);
    }

    private void OnStartup(Entity<CalloutComponent> ent, ref ComponentStartup args)
    {
        ent.Comp.CalloutActionEntity = _actions.AddAction(ent.Owner, ent.Comp.CalloutAction);
    }

    private void OnShutdown(Entity<CalloutComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Comp.CalloutActionEntity);
    }

    private void OnCallout(Entity<CalloutComponent> ent, ref CalloutActionEvent args)
    {
        _actions.StartUseDelay(args.Action);

        _chat.TrySendInGameICMessage(ent.Owner, Loc.GetString(ent.Comp.CalloutString), InGameICChatType.Speak, true, true, checkRadioPrefix: false);

        _audio.PlayEntity(ent.Comp.CalloutSpecifier, Filter.Pvs(ent.Owner), ent.Owner, true, AudioParams.Default);
    }
}