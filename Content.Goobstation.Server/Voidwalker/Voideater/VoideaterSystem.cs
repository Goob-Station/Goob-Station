using Content.Goobstation.Server.Voidwalker.Kidnapping.Voided;
using Content.Shared.StatusEffectNew;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Goobstation.Server.Voidwalker.Voideater;

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
            if (HasComp<VoidedComponent>(entity))
                _status.TryAddStatusEffect(entity, voideater.Comp.SleepingEffectProto, out _, voideater.Comp.SleepDuration);
    }
}
