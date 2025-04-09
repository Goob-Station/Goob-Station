// SPDX-FileCopyrightText: 2024 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Krunklehorn <42424291+Krunklehorn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Overlays;
using Content.Shared.PDA;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;

namespace Content.Client.Overlays;

public sealed class ShowJobIconsSystem : EquipmentHudSystem<ShowJobIconsComponent>
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly AccessReaderSystem _accessReader = default!;

    [ValidatePrototypeId<JobIconPrototype>]
    private const string JobIconForNoId = "JobIconNoId";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<Goobstation.Shared.Wizard.Components.HasJobIconsComponent, GetStatusIconsEvent>(OnGetStatusIconsEvent); // Goob edit
    }

    private void OnGetStatusIconsEvent(EntityUid uid, Goobstation.Shared.Wizard.Components.HasJobIconsComponent _, ref GetStatusIconsEvent ev) // Goob edit
    {
        if (!IsActive)
            return;

        var iconId = JobIconForNoId;

        if (_accessReader.FindAccessItemsInventory(uid, out var items))
        {
            foreach (var item in items)
            {
                // ID Card
                if (TryComp<IdCardComponent>(item, out var id))
                {
                    iconId = id.JobIcon;
                    break;
                }

                // PDA
                if (TryComp<PdaComponent>(item, out var pda)
                    && pda.ContainedId != null
                    && TryComp(pda.ContainedId, out id))
                {
                    iconId = id.JobIcon;
                    break;
                }
            }
        }

        if (_prototype.TryIndex<JobIconPrototype>(iconId, out var iconPrototype))
            ev.StatusIcons.Add(iconPrototype);
        else
            Log.Error($"Invalid job icon prototype: {iconPrototype}");
    }
}
