using System.Numerics;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Sprite;
using Content.Shared.StatusEffectNew;

namespace Content.Trauma.Shared.Shrinking;

public sealed class ShrinkingSystem : EntitySystem
{
    [Dependency] private readonly SharedScaleVisualsSystem _scale = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ShrunkStatusEffectComponent, StatusEffectAppliedEvent>(OnApplied);
        SubscribeLocalEvent<ShrunkStatusEffectComponent, StatusEffectRemovedEvent>(OnRemoved);
    }

    private void OnApplied(Entity<ShrunkStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        _scale.SetSpriteScale(args.Target, new Vector2(0.5f, 0.5f));

        if (!TryComp<DamageableComponent>(args.Target, out var damageable))
            return;

        ent.Comp.OriginalDamageModifierSet = damageable.DamageModifierSetId;
        _damageable.SetDamageModifierSetId(args.Target, ent.Comp.DamageModifierSet);
    }

    private void OnRemoved(Entity<ShrunkStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        _scale.SetSpriteScale(args.Target, new Vector2(1f, 1f));
        _damageable.SetDamageModifierSetId(args.Target, ent.Comp.OriginalDamageModifierSet);
    }
}
