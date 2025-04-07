// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
//
// SPDX-License-Identifier: MIT
namespace Content.Server.Disposal.Tube;

[ByRefEvent]
public record struct GetDisposalsConnectableDirectionsEvent
{
    public Direction[] Connectable;
}