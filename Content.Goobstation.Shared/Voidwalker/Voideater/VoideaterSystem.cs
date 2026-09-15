using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.StatusEffectNew;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Goobstation.Shared.Voidwalker.Voideater;

/// <summary>
/// Handles sleeping entities that are voided when hit with the weapon.
/// </summary>
public sealed class VoideaterSystem : EntitySystem
{
    [Dependency] private readonly StatusEffectsSystem _status = default!;

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VoideaterComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(Entity<VoideaterComponent> voideater, ref MeleeHitEvent args)
    {
        if (!args.IsHit
            || args.HitEntities.Count == 0)
            return;

        foreach (var entity in args.HitEntities)
        {
            if (HasComp<Voided.VoidedComponent>(entity))
            {
                _status.TryAddStatusEffect(entity, voideater.Comp.SleepingEffectProto, out _, voideater.Comp.SleepDuration);
                args.Handled = true;
                return;
            }

            if (TryComp<MobStateComponent>(entity, out var mobState) && mobState.CurrentState == MobState.Critical)
                args.Handled = true; // You're here to KIDNAP them, not MURDER them.
        }

    }

}
