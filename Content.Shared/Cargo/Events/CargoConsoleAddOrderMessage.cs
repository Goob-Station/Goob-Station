// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.Serialization;

namespace Content.Shared.Cargo.Events;

/// <summary>
///     Add order to database.
/// </summary>
[Serializable, NetSerializable]
public sealed class CargoConsoleAddOrderMessage : BoundUserInterfaceMessage
{
    public string Requester;
    public string Reason;
    public string CargoProductId;
    public int Amount;

    public CargoConsoleAddOrderMessage(string requester, string reason, string cargoProductId, int amount)
    {
        Requester = requester;
        Reason = reason;
        CargoProductId = cargoProductId;
        Amount = amount;
    }
}
