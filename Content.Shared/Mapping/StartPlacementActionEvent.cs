// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Actions;

namespace Content.Shared.Mapping;

public sealed partial class StartPlacementActionEvent : InstantActionEvent
{
    [DataField("entityType")]
    public string? EntityType;

    [DataField("tileId")]
    public string? TileId;

    [DataField("placementOption")]
    public string? PlacementOption;

    [DataField("eraser")]
    public bool Eraser;
}