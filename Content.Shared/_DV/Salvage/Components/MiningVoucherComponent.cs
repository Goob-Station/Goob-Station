// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared._DV.Salvage.Systems;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Salvage.Components;

/// <summary>
///     Trigger for purchasing a kit from the Mining Vendors.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(MiningVoucherSystem))]
public sealed partial class MiningVoucherComponent : Component
{
    /// <summary>
    /// Vendor must match this whitelist to be redeemed.
    /// </summary>
    [DataField(required: true)]
    public EntityWhitelist VendorWhitelist;

    /// <summary>
    /// Sound to play when redeeming the voucher.
    /// </summary>
    [DataField]
    public SoundSpecifier? RedeemSound = new SoundPathSpecifier("/Audio/Machines/twobeep.ogg");
}