// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Actions;
﻿using Content.Shared.Maps;
using Robust.Shared.Prototypes;

namespace Content.Shared.Mapping;

public sealed partial class StartPlacementActionEvent : InstantActionEvent
{
    [DataField]
    public EntProtoId? EntityType;

    [DataField]
    public ProtoId<ContentTileDefinition>? TileId;

    [DataField]
    public string? PlacementOption;

    [DataField]
    public bool Eraser;
}