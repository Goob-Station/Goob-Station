// SPDX-FileCopyrightText: 2025 Doctor-Cpu <77215380+Doctor-Cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.WashingMachine.Events;

public sealed partial class WashingMachineIsBeingWashed : EntityEventArgs
{
    public EntityUid WashingMachine;
    public HashSet<EntityUid> Items;

    public WashingMachineIsBeingWashed(EntityUid washingMachine, HashSet<EntityUid> items)
    {
        WashingMachine = washingMachine;
        Items = items;
    }
}
