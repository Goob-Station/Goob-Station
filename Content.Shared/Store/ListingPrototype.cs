// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Repo <47093363+Titian3@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 AJCM <AJCM@tutanota.com>
// SPDX-FileCopyrightText: 2024 ActiveMammmoth <140334666+ActiveMammmoth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Fildrance <fildrance@gmail.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 emmafornash <89596994+emmafornash@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Spatison <137375981+Spatison@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.FixedPoint;
using Content.Shared.Heretic.Prototypes; // Goob
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Store;

// IF SOMEONE IS LOOKING AT THIS,
// AND CONSIDERING MERGING WIZDEN NEWSTORE HERE:
// DO NOT.
// I AM NOT RESPONSIBLE IF YOU DO.
// I WILL NOT FIX YOUR BULLSHIT IF YOU BREAK EVERY SINGLE STORE BASED ANTAG EVER AGAIN.
// regards. :heart:

/// <summary>
///     This is the data object for a store listing which is passed around in code.
///     this allows for prices and features of listings to be dynamically changed in code
///     without having to modify the prototypes.
/// </summary>
[Serializable, NetSerializable]
[Virtual, DataDefinition]
public partial class ListingData : IEquatable<ListingData>, ICloneable
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// The name of the listing. If empty, uses the entity's name (if present)
    /// </summary>
    [DataField]
    public string? Name;

    /// <summary>
    /// The description of the listing. If empty, uses the entity's description (if present)
    /// </summary>
    [DataField]
    public string? Description;

    /// <summary>
    /// The categories that this listing applies to. Used for filtering a listing for a store.
    /// </summary>
    [DataField]
    public List<ProtoId<StoreCategoryPrototype>> Categories = new();

    /// <summary>
    /// The original cost of the listing. FixedPoint2 represents the amount of that currency.
    /// This fields should not be used for getting actual cost of item, as there could be
    /// cost modifiers (due to discounts or surplus). Use Cost property on derived class instead.
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<CurrencyPrototype>, FixedPoint2> Cost = new();

    /// <summary>
    /// Specific customizable conditions that determine whether or not the listing can be purchased.
    /// </summary>
    [NonSerialized]
    [DataField(serverOnly: true)]
    public List<ListingCondition>? Conditions;

    /// <summary>
    /// The icon for the listing. If null, uses the icon for the entity or action.
    /// </summary>
    [DataField]
    public SpriteSpecifier? Icon;

    /// <summary>
    /// The priority for what order the listings will show up in on the menu.
    /// </summary>
    [DataField]
    public int Priority;

    /// <summary>
    /// The entity that is given when the listing is purchased.
    /// </summary>
    [DataField]
    public EntProtoId? ProductEntity;

    /// <summary>
    /// The action that is given when the listing is purchased.
    /// </summary>
    [DataField]
    public EntProtoId? ProductAction;

    /// <summary>
    /// The listing ID of the related upgrade listing. Can be used to link a <see cref="ProductAction"/> to an
    /// upgrade or to use standalone as an upgrade
    /// </summary>
    [DataField]
    public ProtoId<ListingPrototype>? ProductUpgradeId;

    /// <summary>
    /// Keeps track of the current action entity this is tied to, for action upgrades
    /// </summary>
    [DataField]
    [NonSerialized]
    public EntityUid? ProductActionEntity;

    /// <summary>
    /// The event that is broadcast when the listing is purchased.
    /// </summary>
    [DataField(serverOnly: true), NonSerialized] // Goob edit
    public object? ProductEvent;

    // goobstation - heretics
    // i am too tired of making separate systems for knowledge adding
    // and all that shit. i've had like 4 failed attempts
    // so i'm just gonna shitcode my way out of my misery
    [DataField]
    public ProtoId<HereticKnowledgePrototype>? ProductHereticKnowledge;

    [DataField]
    public bool RaiseProductEventOnUser;

    /// <summary>
    /// used internally for tracking how many times an item was purchased.
    /// </summary>
    [DataField]
    public int PurchaseAmount;

    /// <summary>
    /// Used to delay purchase of some items.
    /// </summary>
    [DataField]
    public TimeSpan RestockTime = TimeSpan.Zero;

    // WD START
    [DataField] public int SaleLimit = 1;

    [DataField] public bool SaleBlacklist;

    public int DiscountValue;

    public Dictionary<ProtoId<CurrencyPrototype>, FixedPoint2> OldCost = new();

    [DataField]
    public List<string> Components = new();
    // WD END

    /// <summary>
    /// Whether or not to disable refunding for the store when the listing is purchased from it.
    /// Goob edit: This won't disable refund, but instead you won't be able to refund this listing.
    /// </summary>
    [DataField]
    public bool DisableRefund = false;

    /// <summary>
    /// Goobstation.
    /// When purchased, it will block refunds of these listings.
    /// </summary>
    [DataField]
    public HashSet<ProtoId<ListingPrototype>> BlockRefundListings = new();

    public bool Equals(ListingData? listing)
    {
        if (listing == null)
            return false;

        //simple conditions
        if (Priority != listing.Priority ||
            Name != listing.Name ||
            Description != listing.Description ||
            ProductEntity != listing.ProductEntity ||
            ProductAction != listing.ProductAction ||
            RaiseProductEventOnUser != listing.RaiseProductEventOnUser || // Goobstation
            DisableRefund != listing.DisableRefund || // Goobstation
            RestockTime != listing.RestockTime)
            return false;

        if (ProductEvent != null && listing.ProductEvent != null && ProductEvent.GetType() != listing.ProductEvent.GetType()) // Goobstation
            return false;

        if (Icon != null && !Icon.Equals(listing.Icon))
            return false;

        // Goobstation
        if (!BlockRefundListings.OrderBy(x => x).SequenceEqual(listing.BlockRefundListings.OrderBy(x => x)))
            return false;

        // more complicated conditions that eat perf. these don't really matter
        // as much because you will very rarely have to check these.
        if (!Categories.OrderBy(x => x).SequenceEqual(listing.Categories.OrderBy(x => x)))
            return false;

        if (!Cost.OrderBy(x => x).SequenceEqual(listing.Cost.OrderBy(x => x)))
            return false;

        if ((Conditions != null && listing.Conditions != null) &&
            !Conditions.OrderBy(x => x).SequenceEqual(listing.Conditions.OrderBy(x => x)))
            return false;

        return true;
    }

    /// <summary>
    /// Creates a unique instance of a listing. ALWAWYS USE THIS WHEN ENUMERATING LISTING PROTOTYPES
    /// DON'T BE DUMB AND MODIFY THE PROTOTYPES
    /// </summary>
    /// <returns>A unique copy of the listing data.</returns>
    public object Clone()
    {
        return new ListingData
        {
            ID = ID,
            Name = Name,
            Description = Description,
            Categories = Categories,
            Cost = Cost,
            Conditions = Conditions,
            Icon = Icon,
            Priority = Priority,
            ProductEntity = ProductEntity,
            ProductAction = ProductAction,
            ProductUpgradeId = ProductUpgradeId,
            ProductActionEntity = ProductActionEntity,
            ProductEvent = ProductEvent,
            RaiseProductEventOnUser = RaiseProductEventOnUser, // goob edit
            ProductHereticKnowledge = ProductHereticKnowledge, // goob edit
            DisableRefund = DisableRefund, // goob edit
            BlockRefundListings = BlockRefundListings, // goob edit
            PurchaseAmount = PurchaseAmount,
            RestockTime = RestockTime,
            // WD START
            SaleLimit = SaleLimit,
            SaleBlacklist = SaleBlacklist,
            DiscountValue = DiscountValue,
            OldCost = OldCost,
            Components = Components,
            // WD END
        };
    }
}

/// <summary>
///     Defines a set item listing that is available in a store
/// </summary>
[Prototype("listing")]
[Serializable, NetSerializable]
[DataDefinition]
public sealed partial class ListingPrototype : ListingData, IPrototype;