// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage.Systems;
using Content.Shared.Interaction;
using Content.Shared.Movement.Systems;
using Content.Shared.StepTrigger.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Traps;

public sealed partial class StepTrapSystem : EntitySystem
{
    [Dependency] private SharedStaminaSystem _stamina = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private MovementModStatusSystem _movementMod = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StepTrapComponent, StepTriggeredOnEvent>(OnStepTriggered);
        SubscribeLocalEvent<StepTrapComponent, StepTriggerAttemptEvent>(OnStepAttempt);
    }

    private void OnStepAttempt(Entity<StepTrapComponent> ent, ref StepTriggerAttemptEvent args)
    {
        args.Continue = true;
    }

    private void OnStepTriggered(Entity<StepTrapComponent> ent, ref StepTriggeredOnEvent args)
    {
        var comp = ent.Comp;
        var now = _timing.CurTime;
        if (now < comp.NextTriggerAt)
            return;
        comp.NextTriggerAt = now + TimeSpan.FromSeconds(comp.Cooldown);

        var target = args.Tripper;
        _stamina.TakeStaminaDamage(target, comp.StaminaDamage, source: ent.Owner);
        _movementMod.TryUpdateMovementSpeedModDuration(
            target,
            MovementModStatusSystem.TaserSlowdown,
            comp.SlowdownDuration,
            comp.SlowdownModifier);

        if (comp.Damage is { } damage)
            _damageable.TryChangeDamage(target, damage, origin: ent.Owner);

        if (comp.Sound is { } sound)
            _audio.PlayPvs(sound, ent.Owner);
    }
}
