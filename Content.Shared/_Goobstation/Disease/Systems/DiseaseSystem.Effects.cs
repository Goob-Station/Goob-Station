using Content.Shared.Damage;

namespace Content.Shared.Disease;

public sealed partial class DiseaseSystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;

    private void InitializeEffects()
    {
        SubscribeLocalEvent<DiseaseDamageEffectComponent, DiseaseEffectEvent>(OnDamageEffect);
        SubscribeLocalEvent<DiseaseFightImmunityEffectComponent, DiseaseEffectEvent>(OnFightImmunityEffect);
    }

    private void OnDamageEffect(EntityUid uid, DiseaseDamageEffectComponent effect, DiseaseEffectEvent args)
    {
        _damageable.TryChangeDamage(args.Ent, effect.Damage * args.EffectScale);
    }

    private void OnFightImmunityEffect(EntityUid uid, DiseaseFightImmunityEffectComponent effect, DiseaseEffectEvent args)
    {
        ChangeImmunityProgress(args.Disease.Owner, effect.Amount * args.EffectScale, args.Disease.Comp);
    }
}
