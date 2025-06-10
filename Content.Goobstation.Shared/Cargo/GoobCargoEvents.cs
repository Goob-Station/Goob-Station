// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.Cargo;

public sealed class CargoOrderApprovedEvent(EntityUid orderEntity, string productId, NetEntity requester) : EntityEventArgs
{
    public readonly EntityUid OrderEntity = orderEntity;
    public readonly string ProductId = productId;
    public readonly NetEntity Requester = requester;
}
