// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Projectiles;
using Content.Shared.Emoting;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Standing;
using Content.Shared.Stunnable;
using Robust.Shared.GameStates;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Emoting;

public abstract class SharedAnimatedEmotesSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    private const float DodgeStaminaCost = 20f;
    private const float BorgDodgeBatteryCost = 20f;
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
        base.Update(frameTime);
        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<PendingAnimatedEmoteComponent>();
        while (query.MoveNext(out var uid, out var pending))
        {
            if (now < pending.ExpireTime)
                continue;

            RemCompDeferred<PendingAnimatedEmoteComponent>(uid);
        }
    }

    private void OnBeforeEmote(Entity<AnimatedEmotesComponent> ent, ref BeforeEmoteEvent args)
    {
        if (args.Emote.ID != "Flip") // todo pending emote for other anims.
            return;
        var uid = ent.Owner;

        if (HasComp<BorgChassisComponent>(uid)
            && TryComp<MobStateComponent>(uid, out var state))
        {
            if (state.CurrentState != MobState.Alive)
            {
                args.Cancel();
                return;
            }

            var pendingBorg = EnsureComp<PendingAnimatedEmoteComponent>(uid);
            pendingBorg.ExpireTime = _timing.CurTime + FlipDuration;
            Dirty(uid, pendingBorg);
            return;
        }

        if (!TryComp<StandingStateComponent>(uid, out var standing))
        {
            args.Cancel();
            return;
        }

        if (HasComp<PendingAnimatedEmoteComponent>(uid))
            return;

        if (!standing.Standing
            || HasComp<KnockedDownComponent>(uid)
            || HasComp<StunnedComponent>(uid))
        {
            args.Cancel();
            return;
        }

        var newPending = EnsureComp<PendingAnimatedEmoteComponent>(uid);
        newPending.ExpireTime = _timing.CurTime + FlipDuration;
        Dirty(uid, newPending);
    }

    public void ApplyFlipEffects(EntityUid uid)
    {
        if (!TryComp<PendingAnimatedEmoteComponent>(uid, out var pending))
            return;

        var immunity = EnsureComp<ProjectileImmunityComponent>(uid);
        immunity.ExpireTime = pending.ExpireTime;
        immunity.DodgeEffect = FlipDodgeEffect;

        if (HasComp<BorgChassisComponent>(uid))
            immunity.BatteryCostPerDodge = BorgDodgeBatteryCost;
        else
            immunity.StaminaCostPerDodge = DodgeStaminaCost;

        Dirty(uid, immunity);
    }
}
