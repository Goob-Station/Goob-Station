// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Fildrance <fildrance@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Server.Electrocution;
using Content.Shared.Electrocution;
using Content.Shared.Construction;

namespace Content.Server.Construction.Completions;

[DataDefinition]
public sealed partial class AttemptElectrocute : IGraphAction
{
    public void PerformAction(EntityUid uid, EntityUid? userUid, IEntityManager entityManager)
    {
        if (userUid == null)
            return;

        if (!entityManager.TryGetComponent<ElectrifiedComponent>(uid, out var electrified))
            return;

        var currentValue = electrified.Enabled;
        electrified.Enabled = true;

        entityManager.EntitySysManager.GetEntitySystem<ElectrocutionSystem>().TryDoElectrifiedAct(uid, userUid.Value, electrified: electrified);

        electrified.Enabled = currentValue;
    }
}