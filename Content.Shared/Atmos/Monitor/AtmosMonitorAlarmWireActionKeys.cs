// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT
using Robust.Shared.Serialization;

namespace Content.Shared.Atmos.Monitor;

[Serializable, NetSerializable]
public enum AtmosMonitorAlarmWireActionKeys : byte
{
    Network,
}