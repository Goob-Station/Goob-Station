// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.NPC.Pathfinding;

public enum PathResult : byte
{
    NoPath,
    PartialPath,
    Path,
    Continuing,
}