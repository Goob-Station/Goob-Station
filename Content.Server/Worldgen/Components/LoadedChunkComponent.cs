// SPDX-FileCopyrightText: 2023 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Moony <moony@hellomouse.net>
// SPDX-FileCopyrightText: 2023 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Worldgen.Systems;

namespace Content.Server.Worldgen.Components;

/// <summary>
///     This is used for marking a chunk as loaded.
/// </summary>
[RegisterComponent]
[Access(typeof(WorldControllerSystem))]
public sealed partial class LoadedChunkComponent : Component
{
    /// <summary>
    ///     The current list of entities loading this chunk.
    /// </summary>
    [ViewVariables] public List<EntityUid>? Loaders = null;
}
