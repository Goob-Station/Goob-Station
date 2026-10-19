// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using System;
using System.Numerics;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Maths;
using Robust.Shared.Serialization;

namespace Content.Shared.MalfAI;

[Serializable, NetSerializable]
public sealed class MalfAiViewportOpenEvent : EntityEventArgs
{
    public readonly MapId MapId;
    public readonly Vector2 WorldPosition;
    public readonly Vector2i SizePixels;
    public readonly string Title;
    public readonly float Rotation; // Radians, to match grid north
    public readonly int ZoomLevel; // Number of tiles to show (e.g., 3 for 3x3, 6 for 6x6)
    public readonly NetEntity? AnchorEntity; // The anchor entity to follow

    public MalfAiViewportOpenEvent(MapId mapId, Vector2 worldPosition, Vector2i sizePixels, string title, float rotation, int zoomLevel, NetEntity? anchorEntity = null)
    {
        MapId = mapId;
        WorldPosition = worldPosition;
        SizePixels = sizePixels;
        Title = title;
        Rotation = rotation;
        ZoomLevel = zoomLevel;
        AnchorEntity = anchorEntity;
    }
}

[Serializable, NetSerializable]
public sealed class MalfAiViewportCloseEvent : EntityEventArgs
{
}
