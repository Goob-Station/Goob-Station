// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 J. Brown <DrMelon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Dataset;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared.Store;

// goob edit - fuck newstore
// do not touch unless you want to shoot yourself in the leg
public static class ListingLocalisationHelpers
{
    /// <summary>
    /// ListingData's Name field can be either a localisation string or the actual entity's name.
    /// This function gets a localised name from the localisation string if it exists, and if not, it gets the entity's name.
    /// If neither a localised string exists, or an associated entity name, it will return the value of the "Name" field.
    /// </summary>
    public static string GetLocalisedNameOrEntityName(ListingData listingData, IPrototypeManager prototypeManager)
    {
        var name = string.Empty;

        if (listingData.Name != null)
            name = Loc.GetString(listingData.Name);
        else if (listingData.ProductEntity != null)
            name = prototypeManager.Index(listingData.ProductEntity.Value).Name;

        return name;
    }

    /// <summary>
    /// ListingData's Description field can be either a localisation string or the actual entity's description.
    /// This function gets a localised description from the localisation string if it exists, and if not, it gets the entity's description.
    /// If neither a localised string exists, or an associated entity description, it will return the value of the "Description" field.
    /// </summary>
    public static string GetLocalisedDescriptionOrEntityDescription(ListingData listingData, IPrototypeManager prototypeManager)
    {
        var desc = string.Empty;

        if (listingData.Description != null)
            desc = Loc.GetString(listingData.Description);
        else if (listingData.ProductEntity != null)
            desc = prototypeManager.Index(listingData.ProductEntity.Value).Description;

        // goob edit
        var _protoMan = IoCManager.Resolve<IPrototypeManager>();
        var _rand = IoCManager.Resolve<IRobustRandom>();

        var discountFluff = _rand.Pick(_protoMan.Index<DatasetPrototype>("UplinkDiscountFluff").Values);
        var discountString = $"{Loc.GetString("store-sales-amount", ("amount", listingData.DiscountValue))} {discountFluff}";

        if (listingData.DiscountValue > 0)
            desc += "\n" + discountString;
        else if (listingData.OldCost.Count > 0)
            desc += "\n" + Loc.GetString("store-sales-over");
        // goob edit end

        return desc;
    }
}