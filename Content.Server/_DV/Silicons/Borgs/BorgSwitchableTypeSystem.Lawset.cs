// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._DV.Silicons.Laws;
using Content.Server.Silicons.Laws;
using Content.Shared._DV.Silicons.Laws;
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
    [Dependency] private readonly SlavedBorgSystem _slavedBorg = default!;
    [Dependency] private readonly SiliconLawSystem _law = default!;

    private void ConfigureLawset(EntityUid uid, ProtoId<SiliconLawsetPrototype> id)
    {
        var laws = _law.GetLawset(id);

        if (TryComp<SlavedBorgComponent>(uid, out var slaved))
            _slavedBorg.AddLaw(laws, slaved.Law);

        _law.SetLaws(laws.Laws, uid);

        // re-add law 0 and final law based on new lawset
        if (CompOrNull<EmagSiliconLawComponent>(uid)?.OwnerName != null)
        {
            // raising the event manually to bypass re-emagging checks
            var ev = new SiliconEmaggedEvent(uid);
            RaiseLocalEvent(uid, ref ev);
        }

        // ion storms don't get mirrored because thats basically impossible to track
    }
}