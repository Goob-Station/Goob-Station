// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using System;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Maths;

namespace Content.Server.MalfAI;

/// <summary>
/// Server-only storage for Malf AI viewport settings and cooldown state.
/// </summary>
[RegisterComponent]
public sealed partial class MalfAiViewportComponent : Component
{
    /// <summary>
    /// The last world coordinates chosen by the AI for the viewport, if any.
    /// </summary>
    [DataField]
    public MapCoordinates? Selected;

    /// <summary>
    /// The invisible entity anchored to the grid at the viewport location.
    /// This entity will move with the grid to keep the viewport positioned correctly.
    /// </summary>
    [DataField]
    public EntityUid? ViewportAnchor;

    /// <summary>
    /// Cooldown between setting a new viewport location.
    /// </summary>
    [DataField]
    public TimeSpan SetCooldown = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Absolute game time when the AI may set the viewport again.
    /// </summary>
    [DataField]
    public TimeSpan NextSetAllowedAt;

    /// <summary>
    /// Preferred window size in pixels for the viewport UI.
    /// </summary>
    [DataField]
    public Vector2i WindowSize = (512, 512);

    /// <summary>
    /// Title for the viewport UI.
    /// </summary>
    [DataField]
    public string Title = "AI Viewport";

    /// <summary>
    /// Zoom level for the viewport.
    /// </summary>
    [DataField]
    public int ZoomLevel = 1;

    /// <summary>
    /// Tracks whether the viewport window is currently open on the client.
    /// </summary>
    [DataField]
    public bool IsWindowOpen = false;
}
