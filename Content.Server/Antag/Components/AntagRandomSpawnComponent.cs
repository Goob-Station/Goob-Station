// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server.Antag.Components;

/// <summary>
/// Spawns this rule's antags at random tiles on a station using <c>TryGetRandomTile</c>.
/// Requires <see cref="AntagSelectionComponent"/>.
/// </summary>
[RegisterComponent]
public sealed partial class AntagRandomSpawnComponent : Component;