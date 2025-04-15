// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared.NPC;

[Serializable, NetSerializable]
public sealed class PathPolysRefreshMessage : EntityEventArgs
{
    public NetEntity GridUid;
    public Vector2i Origin;

    /// <summary>
    /// Multi-dimension arrays aren't supported so
    /// </summary>
    public Dictionary<Vector2i, List<DebugPathPoly>> Polys = new();
}