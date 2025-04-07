// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server.Arcade.BlockGame;

[RegisterComponent]
public sealed partial class BlockGameArcadeComponent : Component
{
    /// <summary>
    /// The currently active session of NT-BG.
    /// </summary>
    public BlockGame? Game = null;

    /// <summary>
    /// The player currently playing the active session of NT-BG.
    /// </summary>
    public EntityUid? Player = null;

    /// <summary>
    /// The players currently viewing (but not playing) the active session of NT-BG.
    /// </summary>
    public readonly List<EntityUid> Spectators = new();
}