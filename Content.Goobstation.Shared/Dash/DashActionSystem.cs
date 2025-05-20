// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ted Lukin <66275205+pheenty@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Emoting;
using Content.Shared.Actions;
using Content.Shared.Gravity;
using Content.Shared.Magic.Events;
using Content.Shared.Movement.Components;
using Content.Shared.Throwing;

namespace Content.Goobstation.Shared.Dash;

public sealed class DashActionSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedGravitySystem _gravity = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DashActionEvent>(OnDashAction);

        SubscribeLocalEvent<DashActionComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<DashActionComponent, ComponentShutdown>(OnComponentShutdown);
    }

    private void OnDashAction(DashActionEvent args)
    {
        if (args.Handled)
            return;

        if (args.NeedsGravity && _gravity.IsWeightless(args.Performer))
            return;

        args.Handled = true;
        var vec = (_transform.ToMapCoordinates(args.Target).Position -
                   _transform.GetMapCoordinates(args.Performer).Position).Normalized() * args.Distance;
        var speed = args.Speed;

        if (args.AffectedBySpeed && TryComp<MovementSpeedModifierComponent>(args.Performer, out var speedcomp))
        {
            vec *= speedcomp.CurrentSprintSpeed / speedcomp.BaseSprintSpeed;
            speed *= speedcomp.CurrentSprintSpeed / speedcomp.BaseSprintSpeed;
        }

        _throwing.TryThrow(args.Performer, vec, speed, animated: false);

        if (args.Emote != null && TryComp<AnimatedEmotesComponent>(args.Performer, out var emotes))
        {
            emotes.Emote = args.Emote;
            Dirty(args.Performer, emotes);
        }

        if (args.Speech != null)
        {
            var ev = new SpeakSpellEvent(args.Performer, args.Speech);
            RaiseLocalEvent(ref ev);
        }
    }

    private void OnComponentInit(EntityUid uid, DashActionComponent comp, ref ComponentInit args)
    {
        comp.ActionUid = _actions.AddAction(uid, comp.ActionProto);
    }

    private void OnComponentShutdown(EntityUid uid, DashActionComponent comp, ref ComponentShutdown args)
    {
        _actions.RemoveAction(comp.ActionUid);
    }
}
