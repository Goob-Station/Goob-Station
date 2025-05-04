// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Chat.Systems;
using Content.Server.Pinpointer;
using Content.Shared.Mobs;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.RelayedDeathrattle;

public sealed class RelayedDeathrattleSystem : EntitySystem
{
    [Dependency] private readonly NavMapSystem _navMap = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RelayedDeathrattleComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnMobStateChanged(EntityUid uid, RelayedDeathrattleComponent comp, MobStateChangedEvent args)
    {
        if (comp.Target == null)
            return;

        var posText = FormattedMessage.RemoveMarkupOrThrow(_navMap.GetNearestBeaconString(uid));

        if (args is { NewMobState: MobState.Critical, OldMobState: MobState.Alive })
            _chat.TrySendInGameICMessage(comp.Target.Value, Loc.GetString(comp.CritMessage, ("user", uid), ("position", posText)), InGameICChatType.Speak, hideChat: false);

        if (args.NewMobState == MobState.Dead)
            _chat.TrySendInGameICMessage(comp.Target.Value, Loc.GetString(comp.DeathMessage, ("user", uid), ("position", posText)), InGameICChatType.Speak, hideChat: false);
    }
}
