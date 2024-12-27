using Content.Shared.Access.Systems;
using Content.Shared.DeltaV.Salvage.Systems;
using Content.Shared.Destructible;
using Content.Shared.Popups;
using Content.Shared.Power;
using Content.Shared.Power.EntitySystems;
using Content.Shared.UserInterface;
using Content.Shared.VendingMachines;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared.DeltaV.VendingMachines;

public abstract class SharedShopVendorSystem : EntitySystem
{
    [Dependency] private readonly AccessReaderSystem _access = default!;
    [Dependency] private readonly MiningPointsSystem _points = default!;
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPointLightSystem _light = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PointsVendorComponent, ShopVendorBalanceEvent>(OnPointsBalance);
        SubscribeLocalEvent<PointsVendorComponent, ShopVendorPurchaseEvent>(OnPointsPurchase);

        SubscribeLocalEvent<ShopVendorComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<ShopVendorComponent, BreakageEventArgs>(OnBreak);
        SubscribeLocalEvent<ShopVendorComponent, ActivatableUIOpenAttemptEvent>(OnOpenAttempt);
        Subs.BuiEvents<ShopVendorComponent>(VendingMachineUiKey.Key, subs =>
        {
            subs.Event<ShopVendorPurchaseMessage>(OnPurchase);
        });
    }

    #region Public API

    public uint GetBalance(EntityUid uid, EntityUid user)
    {
        var ev = new ShopVendorBalanceEvent(user);
        RaiseLocalEvent(uid, ref ev);
        return ev.Balance;
    }

    #endregion

    #region Balance adapters

    private void OnPointsBalance(Entity<PointsVendorComponent> ent, ref ShopVendorBalanceEvent args)
    {
        args.Balance = _points.TryFindIdCard(args.User)?.Comp?.Points ?? 0;
    }

    private void OnPointsPurchase(Entity<PointsVendorComponent> ent, ref ShopVendorPurchaseEvent args)
    {
        if (_points.TryFindIdCard(args.User) is {} idCard && _points.RemovePoints(idCard, args.Cost))
            args.Paid = true;
    }

    #endregion

    private void OnPowerChanged(Entity<ShopVendorComponent> ent, ref PowerChangedEvent args)
    {
        UpdateVisuals(ent);
    }

    private void OnBreak(Entity<ShopVendorComponent> ent, ref BreakageEventArgs args)
    {
        ent.Comp.Broken = true;
        UpdateVisuals(ent);
    }

    private void OnOpenAttempt(Entity<ShopVendorComponent> ent, ref ActivatableUIOpenAttemptEvent args)
    {
        if (ent.Comp.Broken)
            args.Cancel();
    }

    private void OnPurchase(Entity<ShopVendorComponent> ent, ref ShopVendorPurchaseMessage args)
    {
        if (ent.Comp.Ejecting != null || ent.Comp.Broken || !_power.IsPowered(ent.Owner))
            return;

        var pack = _proto.Index(ent.Comp.Pack);
        if (args.Index < 0 || args.Index >= pack.Listings.Count)
            return;

        var user = args.Actor;
        if (!_access.IsAllowed(user, ent))
        {
            Deny(ent, user);
            return;
        }

        var listing = pack.Listings[args.Index];
        var ev = new ShopVendorPurchaseEvent(user, listing.Cost);
        RaiseLocalEvent(ent, ref ev);
        if (!ev.Paid)
        {
            Deny(ent, user);
            return;
        }

        ent.Comp.Ejecting = listing.Id;
        ent.Comp.NextEject = Timing.CurTime + ent.Comp.EjectDelay;
        Dirty(ent);

        _audio.PlayPvs(ent.Comp.PurchaseSound, ent);
        UpdateVisuals(ent);

        Log.Debug($"Player {ToPrettyString(user):user} purchased {listing.Id} from {ToPrettyString(ent):vendor}");

        AfterPurchase(ent);
    }

    protected virtual void AfterPurchase(Entity<ShopVendorComponent> ent)
    {
    }

    private void Deny(Entity<ShopVendorComponent> ent, EntityUid user)
    {
        _popup.PopupClient(Loc.GetString("vending-machine-component-try-eject-access-denied"), ent, user);
        if (ent.Comp.Denying)
            return;

        ent.Comp.Denying = true;
        ent.Comp.NextDeny = Timing.CurTime + ent.Comp.DenyDelay;
        Dirty(ent);

        _audio.PlayPvs(ent.Comp.DenySound, ent);
        UpdateVisuals(ent);
    }

    protected void UpdateVisuals(Entity<ShopVendorComponent> ent)
    {
        var state = VendingMachineVisualState.Normal;
        var lit = true;
        if (ent.Comp.Broken)
        {
            state = VendingMachineVisualState.Broken;
            lit = false;
        }
        else if (ent.Comp.Ejecting != null)
        {
            state = VendingMachineVisualState.Eject;
        }
        else if (ent.Comp.Denying)
        {
            state = VendingMachineVisualState.Deny;
        }
        else if (!_power.IsPowered(ent.Owner))
        {
            state = VendingMachineVisualState.Off;
            lit = true;
        }

        _light.SetEnabled(ent, lit);
        _appearance.SetData(ent, VendingMachineVisuals.VisualState, state);
    }
}

/// <summary>
/// Raised on a shop vendor to get its current balance.
/// A currency component sets Balance to whatever it is.
/// </summary>
[ByRefEvent]
public record struct ShopVendorBalanceEvent(EntityUid User, uint Balance = 0);

/// <summary>
/// Raised on a shop vendor when trying to purchase an item.
/// A currency component sets Paid to true if the user successfully paid for it.
/// </summary>
[ByRefEvent]
public record struct ShopVendorPurchaseEvent(EntityUid User, uint Cost, bool Paid = false);
