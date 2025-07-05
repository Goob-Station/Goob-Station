// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._DV.Shipyard.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Shipyard;

[Serializable, NetSerializable]
public enum ShipyardConsoleUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class ShipyardConsoleState(int balance) : BoundUserInterfaceState
{
    public readonly int Balance = balance;
}

/// <summary>
/// Ask the server to purchase a vessel.
/// </summary>
[Serializable, NetSerializable]
public sealed class ShipyardConsolePurchaseMessage(string vessel) : BoundUserInterfaceMessage
{
    public readonly ProtoId<VesselPrototype> Vessel = vessel;
}
