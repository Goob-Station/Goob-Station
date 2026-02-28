// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Shitmed.DelayedDeath;
using Content.Shared._Shitmed.Body.Organ;
using Content.Shared.Body.Systems;
using Content.Server.Popups;
using Content.Shared.Damage;
using Content.Shared.Rejuvenate;
using Content.Shared.Speech;
using Content.Shared.Standing;
using Content.Shared.Stunnable;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;

namespace Content.Server._Shitmed.Body.Systems;

/// <summary>
///     This system handles behavior on entities when they lose their head or their brains are removed.
///     MindComponent fuckery should still be mainly handled on BrainSystem as usual.
/// </summary>
public sealed class DebrainedSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly StandingStateSystem _standingSystem = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DebrainedComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<DebrainedComponent, ComponentRemove>(OnComponentRemove);
        SubscribeLocalEvent<DebrainedComponent, SpeakAttemptEvent>(OnSpeakAttempt);
        SubscribeLocalEvent<DebrainedComponent, StandAttemptEvent>(OnStandAttempt);
        SubscribeLocalEvent<DebrainedComponent, RejuvenateEvent>(OnRejuvenate,
            before: new []{ typeof(DamageableSystem) });
    }

    private void OnComponentInit(EntityUid uid, DebrainedComponent _, ComponentInit args)
    {
        if (TerminatingOrDeleted(uid))
            return;

        EnsureComp<DelayedDeathComponent>(uid);
        EnsureComp<StunnedComponent>(uid);
        _standingSystem.Down(uid);
    }

    private void OnComponentRemove(EntityUid uid, DebrainedComponent _, ComponentRemove args)
    {
        if (TerminatingOrDeleted(uid))
            return;

        RemComp<DelayedDeathComponent>(uid);
        RemComp<StunnedComponent>(uid);
        if (_bodySystem.TryGetBodyOrganEntityComps<HeartComponent>(uid, out var _))
            RemComp<DelayedDeathComponent>(uid);
    }

    private void OnSpeakAttempt(EntityUid uid, DebrainedComponent _, SpeakAttemptEvent args)
    {
        _popupSystem.PopupEntity(Loc.GetString("speech-muted"), uid, uid);
        args.Cancel();
    }

    private void OnStandAttempt(EntityUid uid, DebrainedComponent _, StandAttemptEvent args)
    {
        args.Cancel();
    }

    private void OnRejuvenate(Entity<DebrainedComponent> ent, ref RejuvenateEvent args)
    {
        RemComp(ent, ent.Comp);
    }
}
