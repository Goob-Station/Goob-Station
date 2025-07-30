using Content.Shared.Popups;
using Content.Shared.ActionBlocker;
using Content.Shared.Input;
using Content.Shared.Hands.Components;
using Robust.Shared.Input.Binding;
using Robust.Shared.Player;

namespace Content.Pirate.Shared.OfferItem;

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
        if (session is not { } playerSession)
            return;

        if ((playerSession.AttachedEntity is not { Valid: true } uid || !Exists(uid)) ||
            !_actionBlocker.CanInteract(uid, null))
            return;

        if (!TryComp<OfferItemComponent>(uid, out var offerItem))
            return;

        if (!TryComp<HandsComponent>(uid, out var hands) || hands.ActiveHand == null)
            return;

        offerItem.Item = hands.ActiveHand.HeldEntity;

        if (offerItem.IsInOfferMode == false)
        {
            if (offerItem.Item == null)
            {
                _popup.PopupEntity(Loc.GetString("offer-item-empty-hand"), uid, uid);
                return;
            }

            if (offerItem.Hand == null || offerItem.Target == null)
            {
                offerItem.IsInOfferMode = true;
                offerItem.Hand = hands.ActiveHand.Name;

                Dirty(uid, offerItem);
                return;
            }
        }

        if (offerItem.Target != null)
        {
            UnReceive(offerItem.Target.Value, offerItem: offerItem);
            offerItem.IsInOfferMode = false;
            Dirty(uid, offerItem);
            return;
        }

        UnOffer(uid, offerItem);
    }
}
