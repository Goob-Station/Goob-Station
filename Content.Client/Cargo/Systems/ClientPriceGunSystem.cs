// SPDX-FileCopyrightText: 2024 beck-thompson <107373427+beck-thompson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Dylan Hunter Whittingham <45404433+DylanWhittingham@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 dylanhunter <dylan2.whittingham@live.uwe.ac.uk>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Cargo.Components;
using Content.Shared.Timing;
using Content.Shared.Cargo.Systems;

namespace Content.Client.Cargo.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed class ClientPriceGunSystem : SharedPriceGunSystem
{
    [Dependency] private readonly UseDelaySystem _useDelay = default!;

    protected override bool GetPriceOrBounty(Entity<PriceGunComponent> entity, EntityUid target, EntityUid user)
    {
        if (!TryComp(entity, out UseDelayComponent? useDelay) || _useDelay.IsDelayed((entity, useDelay)))
            return false;

        // It feels worse if the cooldown is predicted but the popup isn't! So only do the cooldown reset on the server.
        return true;
    }
}