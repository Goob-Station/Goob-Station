// SPDX-FileCopyrightText: 2023 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Fildrance <fildrance@gmail.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Store;
using Content.Shared.Store.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.Store.Conditions;

// goob edit - fuck newstore
// do not touch unless you want to shoot yourself in the leg
public sealed partial class BuyBeforeCondition : ListingCondition
{
    /// <summary>
    ///     Required listing(s) needed to purchase before this listing is available
    /// </summary>
    [DataField] // Goob edit
    public HashSet<ProtoId<ListingPrototype>>? Whitelist; // Goob edit

    /// <summary>
    ///     Goobstation.
    ///     If false, only one of the listings needs to be purchased to pass the whitelist.
    ///     If true, all of the listings need to be purchased.
    /// </summary>
    [DataField]
    public bool WhitelistRequireAll = true;

    /// <summary>
    ///     Listing(s) that if bought, block this purchase, if any.
    /// </summary>
    [DataField] // Goobstation
    public HashSet<ProtoId<ListingPrototype>>? Blacklist;

    public override bool Condition(ListingConditionArgs args)
    {
        if (!args.EntityManager.TryGetComponent<StoreComponent>(args.StoreEntity, out var storeComp))
            return false;

        var allListings = storeComp.Listings;

        var purchasesFound = false;

        if (Blacklist != null)
        {
            foreach (var blacklistListing in Blacklist)
            {
                foreach (var listing in allListings)
                {
                    if (listing.ID == blacklistListing.Id && listing.PurchaseAmount > 0)
                        return false;
                }
            }
        }

        if (Whitelist == null) // Goobstation
            return true;

        foreach (var requiredListing in Whitelist)
        {
            foreach (var listing in allListings)
            {
                if (listing.ID == requiredListing.Id)
                {
                    purchasesFound = listing.PurchaseAmount > 0;

                    // Goobstation
                    switch (purchasesFound)
                    {
                        case true when !WhitelistRequireAll:
                            return true;
                        case false when WhitelistRequireAll:
                            return false;
                    }

                    break;
                }
            }
        }

        return purchasesFound;
    }
}