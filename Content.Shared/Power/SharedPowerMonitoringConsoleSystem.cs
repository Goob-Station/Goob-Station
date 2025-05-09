// SPDX-FileCopyrightText: 2023 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Content.Shared.Power;

[UsedImplicitly]
public abstract class SharedPowerMonitoringConsoleSystem : EntitySystem
{
    // Chunk size is limited as we require ChunkSize^2 <= 32 (number of bits in an int)
    public const int ChunkSize = 5;

    /// <summary>
    /// Converts the chunk's tile into a bitflag for the slot.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetFlag(Vector2i relativeTile)
    {
        return 1 << (relativeTile.X * ChunkSize + relativeTile.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2i GetTileFromIndex(int index)
    {
        var x = index / ChunkSize;
        var y = index % ChunkSize;
        return new Vector2i(x, y);
    }
}