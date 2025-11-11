using Content.Shared._CorvaxNext.Alert.Click;
using Content.Shared.Alert;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Robust.Shared.Timing;

namespace Content.Shared._CorvaxGoob.OfferItem;

public abstract partial class SharedOfferItemSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    [ValidatePrototypeId<AlertPrototype>]
    protected const string OfferAlert = "Offer";

    public override void Initialize()
    {
        SubscribeLocalEvent<OfferItemComponent, InteractUsingEvent>(SetInReceiveMode);
        SubscribeLocalEvent<OfferItemComponent, MoveEvent>(OnMove);

        InitializeInteractions();

        SubscribeLocalEvent<OfferItemComponent, AcceptOfferAlertEvent>(OnClickAlertEvent);
    }

    private void OnClickAlertEvent(Entity<OfferItemComponent> ent, ref AcceptOfferAlertEvent ev)
    {
        if (ev.Handled)
            return;

        if (ev.AlertId != OfferAlert)
            return;

        ev.Handled = true;

        Receive(ent!);
    }
    /// <summary>
    /// Accepting the offer and receive item
    /// </summary>
    public void Receive(Entity<OfferItemComponent?> ent)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        if (!Resolve(ent, ref ent.Comp))
            return;

        if (!TryComp<OfferItemComponent>(ent.Comp.Target, out var offerItem))
            return;

        if (offerItem.Hand is null)
            return;

        if (ent.Comp.Target is null)
            return;

        if (!TryComp<HandsComponent>(ent, out var hands))
            return;

        if (offerItem.Item is not null)
        {
            if (!_hands.TryPickup(ent, offerItem.Item.Value, handsComp: hands))
            {
                _popup.PopupClient(Loc.GetString("offer-item-full-hand"), ent, ent);
                return;
            }

            _popup.PopupClient(Loc.GetString("offer-item-give",
                ("item", Identity.Entity(offerItem.Item.Value, EntityManager)),
                ("target", Identity.Entity(ent, EntityManager))), ent.Comp.Target.Value, ent.Comp.Target.Value);

            _popup.PopupPredicted(Loc.GetString("offer-item-give-other",
                    ("user", Identity.Entity(ent.Comp.Target.Value, EntityManager)),
                    ("item", Identity.Entity(offerItem.Item.Value, EntityManager)),
                    ("target", Identity.Entity(ent, EntityManager))),
                ent.Comp.Target.Value,
                ent);
        }

        offerItem.Item = null;
        Dirty(ent);
        UnReceive(ent, ent, offerItem);
    }

    private void SetInReceiveMode(EntityUid uid, OfferItemComponent component, InteractUsingEvent args)
    {
        if (!TryComp<OfferItemComponent>(args.User, out var offerItem))
            return;

        if (args.User == uid || component.IsInReceiveMode || !offerItem.IsInOfferMode || offerItem.IsInReceiveMode && offerItem.Target != uid)
            return;

        component.IsInReceiveMode = true;
        component.Target = args.User;

        Dirty(uid, component);

        offerItem.Target = uid;
        offerItem.IsInOfferMode = false;

        Dirty(args.User, offerItem);

        if (offerItem.Item == null)
            return;

        _popup.PopupPredicted(Loc.GetString("offer-item-try-give",
            ("item", Identity.Entity(offerItem.Item.Value, EntityManager)),
            ("target", Identity.Entity(uid, EntityManager))), component.Target.Value, component.Target.Value);
        _popup.PopupClient(Loc.GetString("offer-item-try-give-target",
            ("user", Identity.Entity(component.Target.Value, EntityManager)),
            ("item", Identity.Entity(offerItem.Item.Value, EntityManager))), component.Target.Value, uid);

        args.Handled = true;
    }

    private void OnMove(EntityUid uid, OfferItemComponent component, MoveEvent args)
    {
        if (component.Target is null ||
            _transform.InRange(args.NewPosition,
                Transform(component.Target.Value).Coordinates,
                component.MaxOfferDistance)
            )
            return;

        UnOffer(uid, component);
    }

    /// <summary>
    /// Resets the <see cref="OfferItemComponent"/> of the user and the target
    /// </summary>
    protected void UnOffer(EntityUid uid, OfferItemComponent component)
    {
        if (!TryComp<HandsComponent>(uid, out var hands) || hands.ActiveHandId is null)
            return;

        if (TryComp<OfferItemComponent>(component.Target, out var offerItem) && component.Target is not null)
        {
            if (component.Item is not null)
            {
                if (!_timing.IsFirstTimePredicted)
                {
                    _popup.PopupClient(Loc.GetString("offer-item-no-give",
                        ("item", Identity.Entity(component.Item.Value, EntityManager)),
                        ("target", Identity.Entity(component.Target.Value, EntityManager))), uid, uid);
                    _popup.PopupEntity(Loc.GetString("offer-item-no-give-target",
                        ("user", Identity.Entity(uid, EntityManager)),
                        ("item", Identity.Entity(component.Item.Value, EntityManager))), uid, component.Target.Value);
                }

            }
            else if (offerItem.Item is not null)
            {
                if (!_timing.IsFirstTimePredicted)
                {
                    _popup.PopupClient(Loc.GetString("offer-item-no-give",
                        ("item", Identity.Entity(offerItem.Item.Value, EntityManager)),
                        ("target", Identity.Entity(uid, EntityManager))), component.Target.Value, component.Target.Value);
                    _popup.PopupEntity(Loc.GetString("offer-item-no-give-target",
                        ("user", Identity.Entity(component.Target.Value, EntityManager)),
                        ("item", Identity.Entity(offerItem.Item.Value, EntityManager))), component.Target.Value, uid);
                }
            }

            offerItem.IsInOfferMode = false;
            offerItem.IsInReceiveMode = false;
            offerItem.Hand = null;
            offerItem.Target = null;
            offerItem.Item = null;

            Dirty(component.Target.Value, offerItem);
        }

        component.IsInOfferMode = false;
        component.IsInReceiveMode = false;
        component.Hand = null;
        component.Target = null;
        component.Item = null;

        Dirty(uid, component);
    }


    /// <summary>
    /// Cancels the transfer of the item
    /// </summary>
    protected void UnReceive(EntityUid uid, OfferItemComponent? component = null, OfferItemComponent? offerItem = null)
    {
        if (component is null && !TryComp(uid, out component))
            return;

        if (offerItem is null && !TryComp(component.Target, out offerItem))
            return;

        if (!TryComp<HandsComponent>(uid, out var hands) || hands.ActiveHandId is null ||
            component.Target is null)
            return;

        if (offerItem.Item is not null)
        {
            _popup.PopupClient(Loc.GetString("offer-item-no-give",
                ("item", Identity.Entity(offerItem.Item.Value, EntityManager)),
                ("target", Identity.Entity(uid, EntityManager))), component.Target.Value, component.Target.Value);
            _popup.PopupEntity(Loc.GetString("offer-item-no-give-target",
                ("user", Identity.Entity(component.Target.Value, EntityManager)),
                ("item", Identity.Entity(offerItem.Item.Value, EntityManager))), component.Target.Value, uid);
        }

        if (!offerItem.IsInReceiveMode)
        {
            offerItem.Target = null;
            component.Target = null;
        }

        offerItem.Item = null;
        offerItem.Hand = null;
        component.IsInReceiveMode = false;

        Dirty(uid, component);
    }

    /// <summary>
    /// Returns true if <see cref="OfferItemComponent.IsInOfferMode"/> = true
    /// </summary>
    protected bool IsInOfferMode(EntityUid? entity, OfferItemComponent? component = null)
    {
        return entity is not null && Resolve(entity.Value, ref component, false) && component.IsInOfferMode;
    }
}
