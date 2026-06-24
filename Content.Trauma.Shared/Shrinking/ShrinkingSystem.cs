// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Sprite;
using Content.Shared.StatusEffectNew;

namespace Content.Trauma.Shared.Shrinking;

public sealed partial class ShrinkingSystem : EntitySystem
{
    [Dependency] private SharedScaleVisualsSystem _scale = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ShrunkStatusEffectComponent, StatusEffectAppliedEvent>(OnApplied);
        SubscribeLocalEvent<ShrunkStatusEffectComponent, StatusEffectRemovedEvent>(OnRemoved);
    }

    private void OnApplied(Entity<ShrunkStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        ent.Comp.OriginalSize = _scale.GetSpriteScale(ent.Owner);
        _scale.SetSpriteScale(args.Target, new Vector2(0.5f, 0.5f));
    }

    private void OnRemoved(Entity<ShrunkStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        _scale.SetSpriteScale(args.Target, ent.Comp.OriginalSize);
    }
}
