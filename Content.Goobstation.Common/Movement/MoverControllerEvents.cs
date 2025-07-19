// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Map;

namespace Content.Goobstation.Common.Movement;

[ByRefEvent]
public readonly record struct MoverControllerCantMoveEvent;

[ByRefEvent]
public readonly record struct MoverControllerGetTileEvent(ITileDefinition? Tile);

public readonly record struct ToggleWalkEvent(bool Walking);