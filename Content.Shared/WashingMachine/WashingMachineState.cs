// SPDX-FileCopyrightText: 2025 Doctor-Cpu <77215380+Doctor-Cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared.WashingMachine;

[Serializable, NetSerializable]
public enum WashingMachineState : byte
{
    Idle,
    Washing,
    Broken
}
