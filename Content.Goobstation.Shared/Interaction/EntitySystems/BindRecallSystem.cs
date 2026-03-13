using Content.Shared.Actions;
using Content.Goobstation.Shared.Interaction.Components;
using Content.Shared.Popups;
using Content.Shared.Verbs;

namespace Content.Shared.Interaction.EntitySystems;

public sealed class BindRecallSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;

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
            Text = Loc.GetString("recall-item-bind"),
            Act = () =>
            {
                var recallComp = EnsureComp<RecallBoundItemComponent>(user);

                // User already has a bound item
                if (recallComp.BoundItem != null)
                {
                    _popup.PopupEntity(Loc.GetString("recall-item-already-have"), user, user);
                    return;
                }

                EntityUid? action = null;

                _actions.AddAction(user, ref action, recallComp.RecallAction);

                if (action != null)
                {
                    recallComp.BoundItem = ent.Owner;
                    recallComp.RecallActionEntity = action;

                    var itemName = Name(ent.Owner);

                    _metaData.SetEntityName(action.Value, Loc.GetString("recall-item-action-name", ("item", itemName)));

                    _metaData.SetEntityDescription(action.Value, Loc.GetString("recall-item-action-desc", ("item", itemName)));
                }

                _popup.PopupEntity(Loc.GetString("recall-item-bound"), user, user);
            }
        };

        args.Verbs.Add(verb);
    }
}
