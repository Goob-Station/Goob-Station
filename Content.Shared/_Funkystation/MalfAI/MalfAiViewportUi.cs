// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using System.Numerics;
using Robust.Shared.Map;
using Robust.Shared.Maths;
using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.MalfAI;

/// <summary>
/// Event sent to the client to open a viewport window at a specific location.
/// </summary>
[Serializable, NetSerializable]
public sealed class MalfAiViewportOpenEvent : EntityEventArgs
{
    public MapId MapId;
    public Vector2 WorldPos;
    public Vector2i SizePixels;
    public string Title;
    public float Rotation;
    public int ZoomLevel;
    public NetEntity? AnchorEntity;

    public MalfAiViewportOpenEvent(
        MapId mapId,
        Vector2 worldPos,
        Vector2i sizePixels,
        string title,
        float rotation,
        int zoomLevel,
        NetEntity? anchorEntity = null)
    {
        MapId = mapId;
        WorldPos = worldPos;
        SizePixels = sizePixels;
        Title = title;
        Rotation = rotation;
        ZoomLevel = zoomLevel;
        AnchorEntity = anchorEntity;
    }
}

/// <summary>
/// Event sent to the client to close the Malf AI viewport window.
/// </summary>
[Serializable, NetSerializable]
public sealed class MalfAiViewportCloseEvent : EntityEventArgs;

/// <summary>
/// Sent from the client to the server when the viewport window is closed locally (e.g. the X button),
/// so the server-side open/closed state stays in sync.
/// </summary>
[Serializable, NetSerializable]
public sealed class MalfAiViewportWindowClosedEvent : EntityEventArgs;
