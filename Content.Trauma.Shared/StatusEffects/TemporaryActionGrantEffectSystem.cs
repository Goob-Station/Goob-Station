// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Content.Shared.StatusEffectNew;

namespace Content.Trauma.Shared.StatusEffects;

public sealed partial class TemporaryActionGrantEffectSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _action = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<TemporaryActionGrantEffectComponent, StatusEffectAppliedEvent>(OnApply);
        SubscribeLocalEvent<TemporaryActionGrantEffectComponent, StatusEffectRemovedEvent>(OnRemove);
    }

    private void OnApply(Entity<TemporaryActionGrantEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        foreach (var action in ent.Comp.ActionPrototypes)
        {
            var actionUid = _action.AddAction(args.Target, action);

            if (actionUid != null)
                ent.Comp.Actions.Add(actionUid.Value);
        }
    }

    private void OnRemove(Entity<TemporaryActionGrantEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        foreach (var action in ent.Comp.Actions)
        {
            _action.RemoveAction(args.Target, action);
        }
    }
}
