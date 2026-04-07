// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Projectiles;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Emoting;
using Content.Shared.Jittering;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Standing;
using Content.Shared.Stunnable;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Emoting;

public abstract class SharedAnimatedEmotesSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;

    private const float FlipStaminaCost = 33f;
    private const string FlipDodgeEffect = "EffectParry";

    public static readonly TimeSpan FlipDuration = TimeSpan.FromMilliseconds(500);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AnimatedEmotesComponent, ComponentGetState>(OnGetState);
        SubscribeLocalEvent<AnimatedEmotesComponent, BeforeEmoteEvent>(OnBeforeEmote);
    }

    private void OnGetState(Entity<AnimatedEmotesComponent> ent, ref ComponentGetState args)
    {
        args.State = new AnimatedEmotesComponentState(ent.Comp.Emote);
    }

    public override void Update(float frameTime)
    {
        // The only reason i'm doing this is cause client sprites desync when stunned while emoting.
        // I figured it'd be overkill but even now it doesn't do it consistently.
        // Fix later. This is a mess.
        base.Update(frameTime);
        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<PendingAnimatedEmoteComponent, StaminaComponent>();
        while (query.MoveNext(out var uid, out var pending, out var stamina))
        {
            if (now < pending.ExpireTime)
                continue;

            if (pending.KnockDownLast)
            {
                _stamina.TakeStaminaDamage(uid, FlipStaminaCost, logDamage: false);
                Dirty(uid, stamina);
                RemCompDeferred<PendingAnimatedEmoteComponent>(uid);
                var ev = new SpriteOverrideEvent();
                RaiseLocalEvent(uid, ref ev);
                return;
            }
            RemCompDeferred<PendingAnimatedEmoteComponent>(uid);
        }
    }

    private void OnBeforeEmote(Entity<AnimatedEmotesComponent> ent, ref BeforeEmoteEvent args)
    {
        if (args.Emote.ID != "Flip") // todo pending emote for other anims.
            return;
        var uid = ent.Owner;

        if (TryComp<BorgChassisComponent>(uid, out var chassis)
            && TryComp<MobStateComponent>(uid, out var state))
        {
            if (state.CurrentState != MobState.Alive)
            {
                args.Cancel();
                return;
            }
            var ev = new BorgFlippingEvent(ent, chassis, FlipStaminaCost, args);
            RaiseLocalEvent(uid, ref ev);
            if (ev.BeforeEmote.Cancelled)
                return;
            var pendingBorg = EnsureComp<PendingAnimatedEmoteComponent>(uid);
            pendingBorg.ExpireTime = _timing.CurTime + FlipDuration;
            Dirty(uid, pendingBorg);
            return;
        }

        if (!TryComp<StaminaComponent>(uid, out var stamina)
            || !TryComp<StandingStateComponent>(uid, out var standing))
        {
            args.Cancel();
            return;
        }
        // if you cancel flipping during a flip, people will just spam perfect flips.
        if (TryComp<PendingAnimatedEmoteComponent>(uid, out var pending))
        {
            if (pending.KnockDownLast)
            {
                args.Cancel();
                return;
            }

            if (stamina.Critical || stamina.StaminaDamage >= stamina.CritThreshold)
            {
                pending.KnockDownLast = true;
                Dirty(uid, pending);
            }
            return;
        }

        if (stamina.Critical
            || !standing.Standing
            || HasComp<KnockedDownComponent>(uid)
            || HasComp<StunnedComponent>(uid))
        {
            args.Cancel();
            return;
        }
        var newPending = EnsureComp<PendingAnimatedEmoteComponent>(uid);
        newPending.ExpireTime = _timing.CurTime + FlipDuration;
        newPending.KnockDownLast = stamina.StaminaDamage + FlipStaminaCost >= stamina.CritThreshold;

        Dirty(uid, newPending);
    }

    public void ApplyFlipEffects(EntityUid uid)
    {
        if (!TryComp<PendingAnimatedEmoteComponent>(uid, out var pending))
            return;

        if (!pending.KnockDownLast && TryComp<StaminaComponent>(uid, out var stamina))
        {
            _stamina.TakeStaminaDamage(uid, FlipStaminaCost, logDamage: false);
            Dirty(uid, stamina);
        }
        var immunity = EnsureComp<ProjectileImmunityComponent>(uid);
        immunity.ExpireTime = pending.ExpireTime;
        immunity.DodgeEffect = FlipDodgeEffect;
        Dirty(uid, immunity);
    }
}
