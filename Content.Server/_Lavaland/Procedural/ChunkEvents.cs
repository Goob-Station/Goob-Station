// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Lavaland.Procedural;

/// <summary>
/// Raised when biome chunk is about to unload.
/// </summary>
[ByRefEvent]
public record struct UnLoadChunkEvent(Vector2i Chunk, bool Cancelled = false);

/// <summary>
/// Raised when biome chunk is about to load.
/// </summary>
[ByRefEvent]
public record struct BeforeLoadChunkEvent(Vector2i Chunk, bool Cancelled = false);
