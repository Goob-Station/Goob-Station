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

        var user = args.User;
        var recallComp = CompOrNull<RecallBoundItemComponent>(user);

        // ------------------
        // Bind verb
        // ------------------
        if (ent.Comp.BoundUser == null)
        {
            var bindVerb = new ActivationVerb
            {
                Text = Loc.GetString("recall-item-bind"),
                Act = () =>
                {
                    var recall = EnsureComp<RecallBoundItemComponent>(user);

                    if (recall.BoundItem != null)
                    {
                        _popup.PopupEntity(Loc.GetString("recall-item-already-have"), user, user);
                        return;
                    }

                    EntityUid? action = null;
                    _actions.AddAction(user, ref action, recall.RecallAction);

                    if (action != null)
                    {
                        recall.BoundItem = ent.Owner;
                        recall.RecallActionEntity = action;

                        ent.Comp.BoundUser = user; // Yes, it probably should not be here, but I am still testing stuff here.
                        Dirty(ent);

                        var itemName = Name(ent.Owner);

                        _metaData.SetEntityName(action.Value,
                            Loc.GetString("recall-item-action-name", ("item", itemName)));

                        _metaData.SetEntityDescription(action.Value,
                            Loc.GetString("recall-item-action-desc", ("item", itemName)));
                    }

                    _popup.PopupEntity(Loc.GetString("recall-item-bound"), user, user);
                }
            };

            args.Verbs.Add(bindVerb);
        }

        // ------------------
        // Unbind verb
        // ------------------
        if (ent.Comp.BoundUser == user && recallComp != null)
        {
            var unbindVerb = new ActivationVerb
            {
                Text = Loc.GetString("recall-item-unbind"),
                Act = () =>
                {
                    if (recallComp.RecallActionEntity != null &&
                        Exists(recallComp.RecallActionEntity.Value))
                    {
                        QueueDel(recallComp.RecallActionEntity.Value);
                    }

                    recallComp.BoundItem = null;
                    recallComp.RecallActionEntity = null;

                    ent.Comp.BoundUser = null; // Yes, it probably should not be here, but I am still testing stuff here.
                    Dirty(ent);

                    _popup.PopupEntity(Loc.GetString("recall-item-unbound"), user, user);
                }
            };

            args.Verbs.Add(unbindVerb);
        }
    }
}
