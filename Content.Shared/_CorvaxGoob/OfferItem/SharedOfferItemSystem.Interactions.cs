using Content.Shared.ActionBlocker;
using Content.Shared.Hands.Components;
using Content.Shared.Input;
using Robust.Shared.Input.Binding;
using Robust.Shared.Player;

namespace Content.Shared._CorvaxGoob.OfferItem;

public abstract partial class SharedOfferItemSystem
{
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;

    private void InitializeInteractions()
    {
        CommandBinds.Builder
            .Bind(ContentKeyFunctions.OfferItem, InputCmdHandler.FromDelegate(SetInOfferMode, handle: false, outsidePrediction: false))
            .Register<SharedOfferItemSystem>();
    }

    public override void Shutdown()
    {
        base.Shutdown();

        CommandBinds.Unregister<SharedOfferItemSystem>();
    }

    private void SetInOfferMode(ICommonSession? session)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        if (session is null)
            return;

        if (session.AttachedEntity is not { Valid: true } uid || !Exists(uid) || !_actionBlocker.CanInteract(uid, null))
            return;

        if (!TryComp<OfferItemComponent>(uid, out var offerItem))
            return;

        if (!TryComp<HandsComponent>(uid, out var hands) || hands.ActiveHandId == null)
            return;

        offerItem.Item = _hands.GetActiveItem(uid);

        if (!offerItem.IsInOfferMode)
        {
            if (offerItem.Item is null)
            {
                _popup.PopupEntity(Loc.GetString("offer-item-empty-hand"), uid, uid);
                return;
            }

            if (offerItem.Hand is null || offerItem.Target is null)
            {
                offerItem.IsInOfferMode = true;
                offerItem.Hand = hands.ActiveHandId;

                Dirty(uid, offerItem);
                return;
            }
        }

        if (offerItem.Target is not null)
        {
            UnReceive(offerItem.Target.Value, offerItem: offerItem);
            offerItem.IsInOfferMode = false;
            Dirty(uid, offerItem);
            return;
        }

        UnOffer(uid, offerItem);
    }
}
