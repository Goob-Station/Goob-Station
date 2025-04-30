// SPDX-FileCopyrightText: 2024 Remuchi <72476615+Remuchi@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Standing;
using Robust.Shared.GameStates;

namespace Content.Shared._EinsteinEngines.TelescopicBaton;

[RegisterComponent, NetworkedComponent]
public sealed partial class KnockdownOnHitComponent : Component
{
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(1);

    [DataField]
    public DropHeldItemsBehavior DropHeldItemsBehavior = DropHeldItemsBehavior.NoDrop;

    [DataField]
    public bool RefreshDuration = true;

    [DataField]
    public bool KnockdownOnHeavyAttack = true; // Goobstation
}
