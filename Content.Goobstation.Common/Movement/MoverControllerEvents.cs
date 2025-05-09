// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoidaBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Map;

namespace Content.Goidastation.Common.Movement;

[ByRefEvent]
public readonly record struct MoverControllerCantMoveEvent;

[ByRefEvent]
public readonly record struct MoverControllerGetTileEvent(ITileDefinition? Tile);
