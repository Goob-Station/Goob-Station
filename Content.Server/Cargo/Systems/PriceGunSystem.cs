// SPDX-FileCopyrightText: 2022 CommieFlowers <rasmus.cedergren@hotmail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 rolfero <45628623+rolfero@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 beck-thompson <107373427+beck-thompson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 blueDev2 <89804215+blueDev2@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 ike709 <ike709@github.com>
// SPDX-FileCopyrightText: 2024 ike709 <ike709@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Dylan Hunter Whittingham <45404433+DylanWhittingham@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 dylanhunter <dylan2.whittingham@live.uwe.ac.uk>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Popups;
using Content.Shared.Cargo.Components;
using Content.Shared.IdentityManagement;
using Content.Shared.Timing;
using Content.Shared.Cargo.Systems;
using Robust.Shared.Audio.Systems;

namespace Content.Server.Cargo.Systems;

public sealed class PriceGunSystem : SharedPriceGunSystem
{
    [Dependency] private readonly UseDelaySystem _useDelay = default!;
    [Dependency] private readonly PricingSystem _pricingSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly CargoSystem _bountySystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    protected override bool GetPriceOrBounty(Entity<PriceGunComponent> entity, EntityUid target, EntityUid user)
    {
        if (!TryComp(entity.Owner, out UseDelayComponent? useDelay) || _useDelay.IsDelayed((entity.Owner, useDelay)))
            return false;
        // Check if we're scanning a bounty crate
        if (_bountySystem.IsBountyComplete(target, out _))
        {
            _popupSystem.PopupEntity(Loc.GetString("price-gun-bounty-complete"), user, user);
        }
        else // Otherwise appraise the price
        {
            var price = _pricingSystem.GetPrice(target);
            _popupSystem.PopupEntity(Loc.GetString("price-gun-pricing-result",
                    ("object", Identity.Entity(target, EntityManager)),
                    ("price", $"{price:F2}")),
                user,
                user);
        }

        _audio.PlayPvs(entity.Comp.AppraisalSound, entity.Owner);
        _useDelay.TryResetDelay((entity.Owner, useDelay));
        return true;
    }
}