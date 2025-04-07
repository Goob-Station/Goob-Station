// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Tiles;

/// <summary>
/// Raised directed on a grid when attempting a floor tile placement.
/// </summary>
[ByRefEvent]
public record struct FloorTileAttemptEvent(bool Cancelled);