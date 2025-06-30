// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Silicons.Laws;
using Content.Shared.Emag.Components;
using Content.Shared.Emag.Systems;
using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.Silicons.Borgs;

/// <summary>
/// Handles lawset patching when switching type.
/// If a borg is made emagged it needs its emag laws carried over.
/// </summary>
public sealed partial class BorgSwitchableTypeSystem
{
    [Dependency] private readonly SiliconLawSystem _law = default!;

    private void ConfigureLawset(EntityUid uid, ProtoId<SiliconLawsetPrototype> id)
    {
        var laws = _law.GetLawset(id);
        _law.SetLaws(laws.Laws, uid);

        // re-add law 0 and final law based on new lawset
        if (CompOrNull<EmagSiliconLawComponent>(uid)?.OwnerName != null)
        {
            // raising the event manually to bypass re-emagging checks
            var ev = new GotEmaggedEvent(uid, EmagType.Interaction); // user wont be used since OwnerName isnt null, safe to pass itself
            RaiseLocalEvent(uid, ref ev);
        }

        // ion storms don't get mirrored because thats basically impossible to track
    }
}