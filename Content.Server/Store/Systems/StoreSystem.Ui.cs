using System.Linq;
using Content.Server._Goobstation.Wizard.Store;
using Content.Server.Actions;
using Content.Server.Administration.Logs;
using Content.Server.Heretic.EntitySystems;
using Content.Server.PDA.Ringer;
using Content.Server.Stack;
using Content.Server.Store.Components;
using Content.Shared._Goobstation.Wizard.Refund; // Goob
using Content.Shared.Actions;
using Content.Shared.Database;
using Content.Shared.FixedPoint;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Heretic; // Goob
using Content.Shared.Heretic.Prototypes; // Goob
using Content.Shared.Mind;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server.Store.Systems;

// goob edit - fuck newstore
// do not touch unless you want to shoot yourself in the leg
public sealed partial class StoreSystem
{
    [Dependency] private readonly IAdminLogManager _admin = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    [Dependency] private readonly ActionUpgradeSystem _actionUpgrade = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly StackSystem _stack = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly HereticKnowledgeSystem _heretic = default!; // goobstation - heretics

    private void InitializeUi()
    {
        SubscribeLocalEvent<StoreComponent, StoreRequestUpdateInterfaceMessage>(OnRequestUpdate);
        SubscribeLocalEvent<StoreComponent, StoreBuyListingMessage>(OnBuyRequest);
        SubscribeLocalEvent<StoreComponent, StoreRequestWithdrawMessage>(OnRequestWithdraw);
        SubscribeLocalEvent<StoreComponent, StoreRequestRefundMessage>(OnRequestRefund);
        SubscribeLocalEvent<StoreComponent, RefundEntityDeletedEvent>(OnRefundEntityDeleted);

        // Goobstation start
        SubscribeLocalEvent<StoreComponent, StoreRefundAllListingsMessage>(OnRefundAll);
        SubscribeLocalEvent<StoreComponent, StoreRefundListingMessage>(OnRefundListing);
        // Goobstation end
    }

    private void OnRefundEntityDeleted(Entity<StoreComponent> ent, ref RefundEntityDeletedEvent args)
    {
        ent.Comp.BoughtEntities.Remove(args.Uid);
    }

    /// <summary>
    /// Toggles the store Ui open and closed
    /// </summary>
    /// <param name="user">the person doing the toggling</param>
    /// <param name="storeEnt">the store being toggled</param>
    /// <param name="component"></param>
    public void ToggleUi(EntityUid user, EntityUid storeEnt, StoreComponent? component = null)
    {
        if (!Resolve(storeEnt, ref component))
            return;

        if (!TryComp<ActorComponent>(user, out var actor))
            return;

        if (!_ui.TryToggleUi(storeEnt, StoreUiKey.Key, actor.PlayerSession))
            return;

        UpdateUserInterface(user, storeEnt, component);
    }

    /// <summary>
    /// Closes the store UI for everyone, if it's open
    /// </summary>
    public void CloseUi(EntityUid uid, StoreComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        _ui.CloseUi(uid, StoreUiKey.Key);
    }

    /// <summary>
    /// Updates the user interface for a store and refreshes the listings
    /// </summary>
    /// <param name="user">The person who if opening the store ui. Listings are filtered based on this.</param>
    /// <param name="store">The store entity itself</param>
    /// <param name="component">The store component being refreshed.</param>
    public void UpdateUserInterface(EntityUid? user, EntityUid store, StoreComponent? component = null)
    {
        if (!Resolve(store, ref component))
            return;

        //this is the person who will be passed into logic for all listing filtering.
        if (user != null) //if we have no "buyer" for this update, then don't update the listings
        {
            component.LastAvailableListings = GetAvailableListings(component.AccountOwner ?? user.Value, store, component).ToHashSet();
        }

        //dictionary for all currencies, including 0 values for currencies on the whitelist
        Dictionary<ProtoId<CurrencyPrototype>, FixedPoint2> allCurrency = new();
        foreach (var supported in component.CurrencyWhitelist)
        {
            allCurrency.Add(supported, FixedPoint2.Zero);

            if (component.Balance.TryGetValue(supported, out var value))
                allCurrency[supported] = value;
        }

        // TODO: if multiple users are supposed to be able to interact with a single BUI & see different
        // stores/listings, this needs to use session specific BUI states.

        // only tell operatives to lock their uplink if it can be locked
        var showFooter = HasComp<RingerUplinkComponent>(store);

        var state = new StoreUpdateState(component.LastAvailableListings, allCurrency, showFooter, component.RefundAllowed);
        _ui.SetUiState(store, StoreUiKey.Key, state);
    }

    private void OnRequestUpdate(EntityUid uid, StoreComponent component, StoreRequestUpdateInterfaceMessage args)
    {
        UpdateUserInterface(args.Actor, GetEntity(args.Entity), component);
    }

    private void BeforeActivatableUiOpen(EntityUid uid, StoreComponent component, BeforeActivatableUIOpenEvent args)
    {
        UpdateUserInterface(args.User, uid, component);
    }

    /// <summary>
    /// Handles whenever a purchase was made.
    /// </summary>
    private void OnBuyRequest(EntityUid uid, StoreComponent component, StoreBuyListingMessage msg)
    {
        var listing = component.Listings.FirstOrDefault(x => x.Equals(msg.Listing));

        if (listing == null) //make sure this listing actually exists
        {
            Log.Debug("listing does not exist");
            return;
        }

        var buyer = msg.Actor;

        //verify that we can actually buy this listing and it wasn't added
        if (!ListingHasCategory(listing, component.Categories))
            return;

        //condition checking because why not
        if (listing.Conditions != null)
        {
            var args = new ListingConditionArgs(component.AccountOwner ?? buyer, uid, listing, EntityManager);
            var conditionsMet = listing.Conditions.All(condition => condition.Condition(args));

            if (!conditionsMet)
                return;
        }

        //check that we have enough money
        // var cost = listing.Cost; // Goobstation
        foreach (var currency in listing.Cost)
        {
            if (!component.Balance.TryGetValue(currency.Key, out var balance) || balance < currency.Value)
            {
                return;
            }
        }

        // if (!IsOnStartingMap(uid, component)) // Goob edit
        //     component.RefundAllowed = false;

        //subtract the cash
        foreach (var (currency, value) in listing.Cost)
        {
            component.Balance[currency] -= value;

            component.BalanceSpent.TryAdd(currency, FixedPoint2.Zero);

            component.BalanceSpent[currency] += value;
        }

        // goobstation - heretics
        // i am too tired of making separate systems for knowledge adding
        // and all that shit. i've had like 4 failed attempts
        // so i'm just gonna shitcode my way out of my misery
        if (listing.ProductHereticKnowledge != null)
        {
            if (TryComp<HereticComponent>(buyer, out var heretic))
                _heretic.AddKnowledge(buyer, heretic, (ProtoId<HereticKnowledgePrototype>) listing.ProductHereticKnowledge);
        }

        //spawn entity
        if (listing.ProductEntity != null)
        {
            var product = Spawn(listing.ProductEntity, Transform(buyer).Coordinates);
            _hands.PickupOrDrop(buyer, product);

            RaiseLocalEvent(product, new ItemPurchasedEvent(buyer));

            HandleRefundComp(uid, component, product, listing.Cost, listing); // Goob edit

            var xForm = Transform(product);

            if (xForm.ChildCount > 0)
            {
                var childEnumerator = xForm.ChildEnumerator;
                while (childEnumerator.MoveNext(out var child))
                {
                    component.BoughtEntities.Add(child);
                }
            }
        }

        //give action
        if (!string.IsNullOrWhiteSpace(listing.ProductAction))
        {
            EntityUid? actionId;
            // I guess we just allow duplicate actions?
            // Allow duplicate actions and just have a single list buy for the buy-once ones.
            if (!_mind.TryGetMind(buyer, out var mind, out _))
                actionId = _actions.AddAction(buyer, listing.ProductAction);
            else
                actionId = _actionContainer.AddAction(mind, listing.ProductAction);

            // Add the newly bought action entity to the list of bought entities
            // And then add that action entity to the relevant product upgrade listing, if applicable
            if (actionId != null)
            {
                HandleRefundComp(uid, component, actionId.Value, listing.Cost, listing); // Goob edit

                if (listing.ProductUpgradeId != null)
                {
                    foreach (var upgradeListing in component.Listings)
                    {
                        if (upgradeListing.ID == listing.ProductUpgradeId)
                        {
                            upgradeListing.ProductActionEntity = actionId.Value;
                            break;
                        }
                    }
                }
            }
        }

        if (listing is { ProductUpgradeId: not null, ProductActionEntity: not null })
        {
            ListingData? originalListing = null; // Goobstation
            var cost = listing.Cost.ToDictionary(); // Goobstation
            if (listing.ProductActionEntity != null)
            {
                if (TryComp(listing.ProductActionEntity.Value, out StoreRefundComponent? storeRefund)) // Goobstation
                {
                    foreach (var (key, value) in storeRefund.BalanceSpent)
                    {
                        cost.TryAdd(key, FixedPoint2.Zero);
                        cost[key] += value;
                    }
                    originalListing = storeRefund.Data;
                }
                component.BoughtEntities.Remove(listing.ProductActionEntity.Value);
            }

            if (!_actionUpgrade.TryUpgradeAction(listing.ProductActionEntity, out var upgradeActionId))
            {
                if (listing.ProductActionEntity != null)
                    HandleRefundComp(uid, component, listing.ProductActionEntity.Value, cost, originalListing, true); // Goob edit

                return;
            }

            listing.ProductActionEntity = upgradeActionId;

            if (upgradeActionId != null)
                HandleRefundComp(uid, component, upgradeActionId.Value, cost, originalListing, true); // Goob edit
        }

        if (listing.ProductEvent != null)
        {
            if (!listing.RaiseProductEventOnUser)
                RaiseLocalEvent(listing.ProductEvent);
            else
                RaiseLocalEvent(buyer, listing.ProductEvent);
        }

        // Goob edit start
        /* if (listing.DisableRefund)
        {
            component.RefundAllowed = false;
        } */
        if (listing.BlockRefundListings.Count > 0)
        {
            foreach (var listingData in component.Listings.Where(x => listing.BlockRefundListings.Contains(x.ID)))
            {
                listingData.DisableRefund = true;
            }
        }
        // Goob edit end

        //log dat shit.
        _admin.Add(LogType.StorePurchase,
            LogImpact.Low,
            $"{ToPrettyString(buyer):player} purchased listing \"{ListingLocalisationHelpers.GetLocalisedNameOrEntityName(listing, _proto)}\" from {ToPrettyString(uid)}");

        listing.PurchaseAmount++; //track how many times something has been purchased
        _audio.PlayEntity(component.BuySuccessSound, msg.Actor, uid); //cha-ching!

        //WD EDIT START
        if (listing.SaleLimit != 0 && listing.DiscountValue > 0 && listing.PurchaseAmount >= listing.SaleLimit)
        {
            listing.DiscountValue = 0;
            listing.Cost = listing.OldCost;
        }
        //WD EDIT END

        UpdateUserInterface(buyer, uid, component);
        UpdateRefundUserInterface(uid, component); // Goobstation
    }

    /// <summary>
    /// Handles dispensing the currency you requested to be withdrawn.
    /// </summary>
    /// <remarks>
    /// This would need to be done should a currency with decimal values need to use it.
    /// not quite sure how to handle that
    /// </remarks>
    private void OnRequestWithdraw(EntityUid uid, StoreComponent component, StoreRequestWithdrawMessage msg)
    {
        if (msg.Amount <= 0)
            return;

        //make sure we have enough cash in the bank and we actually support this currency
        if (!component.Balance.TryGetValue(msg.Currency, out var currentAmount) || currentAmount < msg.Amount)
            return;

        //make sure a malicious client didn't send us random shit
        if (!_proto.TryIndex<CurrencyPrototype>(msg.Currency, out var proto))
            return;

        //we need an actually valid entity to spawn. This check has been done earlier, but just in case.
        if (proto.Cash == null || !proto.CanWithdraw)
            return;

        var buyer = msg.Actor;

        FixedPoint2 amountRemaining = msg.Amount;
        var coordinates = Transform(buyer).Coordinates;

        var sortedCashValues = proto.Cash.Keys.OrderByDescending(x => x).ToList();
        foreach (var value in sortedCashValues)
        {
            var cashId = proto.Cash[value];
            var amountToSpawn = (int) MathF.Floor((float) (amountRemaining / value));
            var ents = _stack.SpawnMultiple(cashId, amountToSpawn, coordinates);
            if (ents.FirstOrDefault() is {} ent)
                _hands.PickupOrDrop(buyer, ent);
            amountRemaining -= value * amountToSpawn;
        }

        component.Balance[msg.Currency] -= msg.Amount;
        UpdateUserInterface(buyer, uid, component);
    }

    private void OnRequestRefund(EntityUid uid, StoreComponent component, StoreRequestRefundMessage args)
    {
        // TODO: Remove guardian/holopara

        if (args.Actor is not { Valid: true } buyer)
            return;

        // Goob edit start
        if (!_ui.HasUi(uid, RefundUiKey.Key))
            component.RefundAllowed = false;

        if (!component.RefundAllowed)
            _ui.CloseUi(uid, RefundUiKey.Key);

        if (!_ui.IsUiOpen(uid, RefundUiKey.Key, buyer))
            _ui.OpenUi(uid, RefundUiKey.Key, buyer);
        else
        {
            _ui.CloseUi(uid, RefundUiKey.Key, buyer);
            return;
        }

        UpdateRefundUserInterface(uid, component);

        /* if (!IsOnStartingMap(uid, component))
        {
            component.RefundAllowed = false;
        }

        if (!component.RefundAllowed || component.BoughtEntities.Count == 0)
            return;

        _admin.Add(LogType.StoreRefund, LogImpact.Low, $"{ToPrettyString(buyer):player} has refunded their purchases from {ToPrettyString(uid):store}");

        for (var i = component.BoughtEntities.Count - 1; i >= 0; i--)
        {
            var purchase = component.BoughtEntities[i];

            if (!Exists(purchase))
                continue;

            component.BoughtEntities.RemoveAt(i);

            if (_actions.TryGetActionData(purchase, out var actionComponent, logError: false))
            {
                _actionContainer.RemoveAction(purchase, actionComponent);
            }

            EntityManager.DeleteEntity(purchase);
        }

        component.BoughtEntities.Clear();

        foreach (var (currency, value) in component.BalanceSpent)
        {
            component.Balance[currency] += value;
        }

        // Reset store back to its original state
        RefreshAllListings(component);
        component.BalanceSpent = new();
        UpdateUserInterface(buyer, uid, component); */

        // Goob edit end
    }

    // Goobstation start
    private void UpdateRefundUserInterface(EntityUid uid, StoreComponent component)
    {
        if (!IsOnStartingMap(uid, component))
            _ui.SetUiState(uid, RefundUiKey.Key, new StoreRefundState(new(), true));
        else
        {
            List<RefundListingData> listings = new();
            foreach (var bought in component.BoughtEntities)
            {
                if (!Exists(bought) || !TryComp(bought, out StoreRefundComponent? refundComp) ||
                    refundComp.Data == null || refundComp.StoreEntity != uid || refundComp.Data.DisableRefund)
                    continue;

                var name = ListingLocalisationHelpers.GetLocalisedNameOrEntityName(refundComp.Data, _proto);
                listings.Add(new RefundListingData(GetNetEntity(bought), name));
            }

            _ui.SetUiState(uid, RefundUiKey.Key, new StoreRefundState(listings, false));
        }
    }

    private bool RefundListing(EntityUid uid, StoreComponent component, EntityUid boughtEntity, EntityUid buyer, bool log)
    {
        if (!IsOnStartingMap(uid, component) || !Exists(boughtEntity) ||
            !TryComp(boughtEntity, out StoreRefundComponent? refundComp) || refundComp.Data == null ||
            refundComp.StoreEntity != uid || refundComp.Data.DisableRefund)
            return false;

        if (log)
            _admin.Add(LogType.StoreRefund, LogImpact.Low, $"{ToPrettyString(buyer):player} has refunded {ToPrettyString(boughtEntity):purchase} from {ToPrettyString(uid):store}");

        foreach (var (currency, value) in refundComp.BalanceSpent)
        {
            component.Balance.TryAdd(currency, FixedPoint2.Zero);
            component.Balance[currency] += value;

            if (component.BalanceSpent.ContainsKey(currency))
                component.BalanceSpent[currency] -= value;
        }

        if (refundComp.Data.ProductUpgradeId != null)
        {
            foreach (var upgradeListing in component.Listings.Where(upgradeListing =>
                         upgradeListing.ID == refundComp.Data.ProductUpgradeId))
            {
                upgradeListing.PurchaseAmount = 0;
                break;
            }
        }

        component.BoughtEntities.Remove(boughtEntity);

        if (_actions.TryGetActionData(boughtEntity, out var actionComponent, logError: false))
            _actionContainer.RemoveAction(boughtEntity, actionComponent);

        refundComp.Data.PurchaseAmount = Math.Max(0, refundComp.Data.PurchaseAmount - 1);

        Del(boughtEntity);

        return true;
    }

    private void OnRefundListing(Entity<StoreComponent> ent, ref StoreRefundListingMessage args)
    {
        if (args.Actor is not { Valid: true } buyer)
            return;

        var (uid, component) = ent;

        var listing = GetEntity(args.ListingEntity);

        if (RefundListing(uid, component, listing, buyer, true))
            UpdateUserInterface(buyer, uid, component);

        UpdateRefundUserInterface(uid, component);
    }

    private void OnRefundAll(Entity<StoreComponent> ent, ref StoreRefundAllListingsMessage args)
    {
        if (args.Actor is not { Valid: true } buyer)
            return;

        var (uid, component) = ent;

        if (!IsOnStartingMap(uid, component) || !component.RefundAllowed || component.BoughtEntities.Count == 0)
        {
            UpdateRefundUserInterface(uid, component);
            return;
        }

        _admin.Add(LogType.StoreRefund, LogImpact.Low, $"{ToPrettyString(buyer):player} has refunded their purchases from {ToPrettyString(uid):store}");

        for (var i = component.BoughtEntities.Count - 1; i >= 0; i--)
        {
            var purchase = component.BoughtEntities[i];

            RefundListing(uid, component, purchase, buyer, false);
        }

        UpdateUserInterface(buyer, uid, component);
        UpdateRefundUserInterface(uid, component);
    }

    public static void DisableListingRefund(ListingData? data)
    {
        if (data != null)
            data.DisableRefund = true;
    }
    // Goobstation end

    private void HandleRefundComp(EntityUid uid, StoreComponent component, EntityUid purchase, Dictionary<ProtoId<CurrencyPrototype>, FixedPoint2> cost, ListingData? data, bool overrideCost = false) // Goob edit
    {
        component.BoughtEntities.Add(purchase);
        var refundComp = EnsureComp<StoreRefundComponent>(purchase);
        refundComp.StoreEntity = uid;
        // Goobstation start
        if (overrideCost)
            refundComp.BalanceSpent = cost;
        else
        {
            foreach (var (key, value) in cost)
            {
                refundComp.BalanceSpent.TryAdd(key, FixedPoint2.Zero);
                refundComp.BalanceSpent[key] += value;
            }
        }

        if (data != null)
            refundComp.Data = data;
        // Goobstation end
    }

    private bool IsOnStartingMap(EntityUid store, StoreComponent component)
    {
        var xform = Transform(store);
        return component.StartingMap == xform.MapUid;
    }

    /// <summary>
    ///     Disables refunds for this store
    /// </summary>
    public void DisableRefund(EntityUid store, StoreComponent? component = null)
    {
        if (!Resolve(store, ref component))
            return;

        component.RefundAllowed = false;
    }
}
