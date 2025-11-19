// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Server.NPC.Pathfinding;

public enum PathResult : byte
{
    NoPath,
    PartialPath,
    Path,
    Continuing,
}
