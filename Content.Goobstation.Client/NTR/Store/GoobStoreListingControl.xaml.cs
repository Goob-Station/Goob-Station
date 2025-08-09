// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 LuciferEOS <stepanteliatnik2022@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Store;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.NTR;

public sealed partial class GoobStoreListingControl : Control
{

    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    // private readonly ListingData _data;
    // _
    // private readonly TimeSpan _initialRestockTime; // goob edit
    // _initialRestockTime = data.RestockTime; // goob edit
    private void OnPurchase(ListingData listing)
    {
        if (!_prototype.TryIndex<ListingPrototype>(listing.ID, out var prototype))
            return;

        if (prototype.ResetRestockOnPurchase)
            listing.RestockTime = _timing.CurTime + prototype.RestockDuration;
    }
}
