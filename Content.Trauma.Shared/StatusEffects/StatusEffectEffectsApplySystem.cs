// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Content.Shared.StatusEffectNew;

namespace Content.Trauma.Shared.StatusEffects;

public sealed partial class StatusEffectEffectsApplySystem : EntitySystem
{
    [Dependency] private SharedEntityEffectsSystem _effects = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StatusEffectEffectsApplyComponent, StatusEffectAppliedEvent>(OnApplied);
        SubscribeLocalEvent<StatusEffectEffectsApplyComponent, StatusEffectRemovedEvent>(OnRemoval);
    }

    private void OnApplied(Entity<StatusEffectEffectsApplyComponent> ent, ref StatusEffectAppliedEvent args)
    {
        if (ent.Comp.EffectsOnApply is not { } effectsOnApply)
            return;

        _effects.ApplyEffects(args.Target, effectsOnApply);
    }

    private void OnRemoval(Entity<StatusEffectEffectsApplyComponent> ent, ref StatusEffectRemovedEvent args)
    {
        if (ent.Comp.EffectsOnRemoval is not { } effectsOnRemoval)
            return;

        _effects.ApplyEffects(args.Target, effectsOnRemoval);
    }
}
