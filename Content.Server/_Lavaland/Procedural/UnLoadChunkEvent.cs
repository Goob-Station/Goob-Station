// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Lavaland.Procedural;

/// <summary>
/// Lavaland: Raised when biome chunk is about to unload.
/// </summary>
public sealed class UnLoadChunkEvent : CancellableEntityEventArgs
{
    public Vector2i Chunk { get; set; }

    public UnLoadChunkEvent(Vector2i chunk)
    {
        Chunk = chunk;
    }
}