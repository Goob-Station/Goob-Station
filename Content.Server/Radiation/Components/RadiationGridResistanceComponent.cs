// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Radiation.Systems;

namespace Content.Server.Radiation.Components;

/// <summary>
///     Grid component that stores radiation resistance of <see cref="RadiationBlockerComponent"/> per tile.
/// </summary>
[RegisterComponent]
[Access(typeof(RadiationSystem), Other = AccessPermissions.ReadExecute)]
public sealed partial class RadiationGridResistanceComponent : Component
{
    /// <summary>
    ///     Radiation resistance per tile.
    /// </summary>
    public readonly Dictionary<Vector2i, float> ResistancePerTile = new();
}