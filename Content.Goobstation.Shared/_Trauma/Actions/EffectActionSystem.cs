// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions.Events;
using Content.Shared.EntityEffects;

namespace Content.Goobstation.Shared._Trauma.Actions;

public sealed partial class EffectActionSystem : EntitySystem
{
    [Dependency] private SharedEntityEffectsSystem _effects = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EffectActionComponent, ActionPerformedEvent>(OnActionPerformed);
        SubscribeLocalEvent<EffectInstantActionEvent>(OnInstantAction);
        SubscribeLocalEvent<EffectTargetActionEvent>(OnTargetAction);
    }

    private void OnActionPerformed(Entity<EffectActionComponent> ent, ref ActionPerformedEvent args)
    {
        var user = args.Performer;
        if (ent.Comp.OnPerformed)
            _effects.ApplyEffects(user, ent.Comp.Effects, user: user);
    }

    private void OnInstantAction(EffectInstantActionEvent args)
    {
        if (args.Handled || !TryComp<EffectActionComponent>(args.Action, out var comp))
            return;

        var user = args.Performer;
        _effects.ApplyEffects(user, comp.Effects, user: user);
        args.Handled = true;
    }

    private void OnTargetAction(EffectTargetActionEvent args)
    {
        if (args.Handled || !TryComp<EffectActionComponent>(args.Action, out var comp))
            return;

        _effects.ApplyEffects(args.Target, comp.Effects, user: args.Performer);
        args.Handled = true;
    }
}
