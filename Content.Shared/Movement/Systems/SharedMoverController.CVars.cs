// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 MarkerWicker <markerWicker@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.CCVar;
using Content.Shared.Mind.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Events;
using Robust.Shared.Configuration;
using Robust.Shared.Player;

namespace Content.Shared.Movement.Systems;

public abstract partial class SharedMoverController
{
    [Dependency] private readonly INetConfigurationManager _netConfig = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;

    private void InitializeCVars()
    {
        SubscribeLocalEvent<InputMoverComponent, MindAddedMessage>(OnMindAdded);
        SubscribeLocalEvent<InputMoverComponent, MindRemovedMessage>(OnMindRemoved);
        SubscribeNetworkEvent<UpdateInputCVarsMessage>(OnUpdateCVars);
    }

    private void OnMindAdded(Entity<InputMoverComponent> ent, ref MindAddedMessage args)
    {
        if (!_player.TryGetSessionById(args.Mind.Comp.UserId, out var session)) return;

        if (session.Channel is not { } channel) return;

        ent.Comp.DefaultSprinting = _netConfig.GetClientCVar(channel, CCVars.DefaultWalk);
        RaiseLocalEvent(ent, new SprintingInputEvent(ent)); // WD EDIT
    }

    private void OnMindRemoved(Entity<InputMoverComponent> ent, ref MindRemovedMessage args)
    {
        // If it's an ai-controlled mob, we probably want them sprinting by default.
        ent.Comp.DefaultSprinting = true;
    }

    private void OnUpdateCVars(UpdateInputCVarsMessage msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } uid || !TryComp<InputMoverComponent>(uid, out var mover))
            return;

        mover.DefaultSprinting = _netConfig.GetClientCVar(args.SenderSession.Channel, CCVars.DefaultWalk);
        RaiseLocalEvent(uid, new SprintingInputEvent((uid, mover))); // WD EDIT
    }
}
