using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Goobstation.Shared.Interaction.Components;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using System;

namespace Content.Shared.Interaction.EntitySystems;

public sealed class BindRecallSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly EntityManager _entMan = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BoundRecallComponent, GetVerbsEvent<ActivationVerb>>(OnGetVerb);
    }

    private void OnGetVerb(Entity<BoundRecallComponent> ent, ref GetVerbsEvent<ActivationVerb> args)
    {
        if (!args.CanInteract)
            return;

        if (ent.Comp.BoundUser != null)
            return;

        var user = args.User;

        var verb = new ActivationVerb
        {
            Text = "Bind Item",
            Act = () =>
            {
                var recallComp = EnsureComp<RecallBoundItemComponent>(user);

                if (recallComp.BoundItems.ContainsKey(ent.Owner))
                {
                    _popup.PopupEntity(Loc.GetString("recall-item-already-bound"), user, user);
                    return;
                }

                ent.Comp.BoundUser = user;

                EntityUid? action = null;

                _actions.AddAction(user, ref action, ent.Comp.RecallAction);

                if (action != null)
                {
                    recallComp.BoundItems[ent.Owner] = action.Value;

                    if (ent.Comp.Icon != null)
                        _actions.SetIcon(action.Value, ent.Comp.Icon);
                }

                _popup.PopupEntity(Loc.GetString("recall-item-bound"), user, user);
            }
        };

        args.Verbs.Add(verb);
    }
}
