// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Content.Shared.Nutrition;
using Content.Shared.Whitelist;

namespace Content.Trauma.Shared.EffectOnEaten;

public sealed partial class EntityEffectOnEatenSystem : EntitySystem
{
    [Dependency] private SharedEntityEffectsSystem _effects = default!;
    [Dependency] private EntityWhitelistSystem _whitelist = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EntityEffectOnEatenComponent, FullyEatenEvent>(OnEaten);
    }

    private void OnEaten(Entity<EntityEffectOnEatenComponent> ent, ref FullyEatenEvent args)
    {
        if (ent.Comp.Whitelist is not null && _whitelist.IsWhitelistFail(ent.Comp.Whitelist, args.Eater))
            return;

        if (ent.Comp.EntityWhitelist is null)
        {
            _effects.ApplyEffects(args.Eater, ent.Comp.Effects, ent.Comp.Scale);
            return;
        }

        var isAllowed = false;

        if (Prototype(args.Eater) is not {} id)
            return;

        foreach (var entity in ent.Comp.EntityWhitelist)
        {
            if (entity.Id == id.ID)
            {
                isAllowed = true;
                break;
            }
        }

        if (!isAllowed)
            return;

        _effects.ApplyEffects(args.Eater, ent.Comp.Effects, ent.Comp.Scale);
    }
}
