// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2025 Rinary <72972221+Rinary1@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 ss14-Starlight <ss14-Starlight@outlook.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Containers;

namespace Content.Shared.VentCrawler.Tube.Components;

/// <summary>
/// A component representing a vent that you can crawl through
/// </summary>
[RegisterComponent]
public sealed partial class VentCrawlerTubeComponent : Component
{
    [DataField]
    public string ContainerId { get; set; } = "VentCrawlerTube";

    [DataField]
    public bool Connected = true;

    [ViewVariables]
    public Container Contents { get; set; } = null!;
}

[ByRefEvent]
public record struct GetVentCrawlingsConnectableDirectionsEvent
{
    public Direction[] Connectable;
}