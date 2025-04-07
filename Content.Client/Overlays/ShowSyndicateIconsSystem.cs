// SPDX-FileCopyrightText: 2024 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 IntegerTempest <30300017+IntegerTempest@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Krunklehorn <42424291+Krunklehorn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 PrPleGoo <PrPleGoo@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Overlays;
using Content.Shared.NukeOps;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;

namespace Content.Client.Overlays;

public sealed class ShowSyndicateIconsSystem : EquipmentHudSystem<ShowSyndicateIconsComponent>
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NukeOperativeComponent, GetStatusIconsEvent>(OnGetStatusIconsEvent);
    }

    private void OnGetStatusIconsEvent(EntityUid uid, NukeOperativeComponent component, ref GetStatusIconsEvent ev)
    {
        if (!IsActive)
            return;

        if (_prototype.TryIndex<FactionIconPrototype>(component.SyndStatusIcon, out var iconPrototype))
            ev.StatusIcons.Add(iconPrototype);
    }
}
