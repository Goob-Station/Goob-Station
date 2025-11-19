// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.Map;

namespace Content.Server.NPC.Pathfinding;

public sealed class PathfindingRequestEvent : EntityEventArgs
{
    public EntityCoordinates Start;
    public EntityCoordinates End;

    // TODO: Need stuff like can we break shit, can we pry, collision mask, etc
}
