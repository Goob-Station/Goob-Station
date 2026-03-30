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
using Robust.Shared.GameStates;
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

    private void OnBeforeEmote(Entity<AnimatedEmotesComponent> ent, ref BeforeEmoteEvent args)
    {
        if (args.Emote.ID != "Flip")
            return;

        if (!TryComp<StaminaComponent>(ent, out var stamina))
            return;

        if (stamina.Critical || stamina.StaminaDamage + FlipStaminaCost >= stamina.CritThreshold)
            args.Cancel();
    }

    public void ApplyFlipEffects(EntityUid uid)
    {
        _stamina.TakeStaminaDamage(uid, FlipStaminaCost, logDamage: false);

        var immunity = EnsureComp<ProjectileImmunityComponent>(uid);
        immunity.ExpireTime = _timing.CurTime + FlipDuration;
        immunity.DodgeEffect = FlipDodgeEffect;
        Dirty(uid, immunity);
    }
}
