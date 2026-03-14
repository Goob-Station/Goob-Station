using Content.Shared.Actions;
using Content.Shared.Hands.EntitySystems;
using Content.Goobstation.Shared.Interaction;
using Content.Goobstation.Shared.Interaction.Components;
using Content.Shared.Popups;

namespace Content.Goobstation.Shared.Interaction.EntitySystems;

public sealed class RecallItemSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<RecallBoundItemComponent, RecallBoundItemEvent>(OnRecall);
        SubscribeLocalEvent<BoundRecallComponent, EntityTerminatingEvent>(OnBoundItemDeleted);
    }

    private void OnRecall(Entity<RecallBoundItemComponent> ent, ref RecallBoundItemEvent args)
    {
        var user = args.Performer;

        if (ent.Comp.BoundItem == null)
            return;

        var item = ent.Comp.BoundItem.Value;

        if (_hands.IsHolding(user, item))
        {
            _popup.PopupEntity(Loc.GetString("recall-item-already-held"), user, user);
            args.Handled = true;
            return;
        }

        if (_hands.TryPickupAnyHand(user, item))
            _popup.PopupEntity(Loc.GetString("recall-item-success"), user, user);
        else
            _popup.PopupEntity(Loc.GetString("recall-item-hands-full"), user, user);

        args.Handled = true;
    }

    private void OnBoundItemDeleted(Entity<BoundRecallComponent> ent, ref EntityTerminatingEvent args)
    {
        var item = ent.Owner;

        var query = EntityQueryEnumerator<RecallBoundItemComponent>();

        while (query.MoveNext(out var userUid, out var recallComp))
        {
            if (recallComp.BoundItem != item)
                continue;

            recallComp.BoundItem = null;

            if (recallComp.RecallActionEntity != null && Exists(recallComp.RecallActionEntity.Value))
                QueueDel(recallComp.RecallActionEntity.Value);

            recallComp.RecallActionEntity = null;

            Dirty(userUid, recallComp);
        }
    }
}
