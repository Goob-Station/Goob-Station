// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Map;

namespace Content.Server.NPC.Pathfinding;

public sealed class PathfindingRequestEvent : EntityEventArgs
{
    public EntityCoordinates Start;
    public EntityCoordinates End;

    // TODO: Need stuff like can we break shit, can we pry, collision mask, etc
}