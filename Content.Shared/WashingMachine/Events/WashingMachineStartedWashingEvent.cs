// SPDX-FileCopyrightText: 2025 Doctor-Cpu <77215380+Doctor-Cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.WashingMachine.Events;

public sealed partial class WashingMachineStartedWashingEvent : EntityEventArgs
{
    public HashSet<EntityUid> Items;

    public WashingMachineStartedWashingEvent(HashSet<EntityUid> items)
    {
        Items = items;
    }
}
