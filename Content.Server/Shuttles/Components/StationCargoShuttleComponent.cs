// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Utility;

namespace Content.Server.Shuttles.Components;

/// <summary>
/// GridSpawnComponent but for cargo shuttles
/// <remarks>
/// This exists so we don't need to make 1 change to GridSpawn for every single station's unique shuttles.
/// </remarks>
/// </summary>
[RegisterComponent]
public sealed partial class StationCargoShuttleComponent : Component
{
    // If you add more than just make an abstract comp, split them, then use overloads in the system.
    // YAML is filled out so mappers don't have to read here.

    [DataField(required: true)]
    public ResPath Path = new("/Maps/Shuttles/cargo.yml");
}