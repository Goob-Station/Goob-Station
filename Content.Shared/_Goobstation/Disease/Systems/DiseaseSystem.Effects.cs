using Content.Shared.Damage;

namespace Content.Shared.Disease;

public sealed partial class DiseaseSystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;

    private void InitializeEffects()
    {
        SubscribeLocalEvent<DiseaseDamageEffectComponent, DiseaseEffectEvent>(OnDamageEffect);
    }

    private void OnDamageEffect(EntityUid uid, DiseaseDamageEffectComponent damageEffect, DiseaseEffectEvent args)
    {
        _damageable.TryChangeDamage(args.Ent, damageEffect.Damage);
    }
}
