// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Map;

namespace Content.Server._Funkystation.MalfAI;

/// <summary>
/// Tracks a Malf AI viewport anchor entity and its state.
/// </summary>
[RegisterComponent]
public sealed partial class MalfAiViewportComponent : Component
{
    /// <summary>
    /// The map coordinates the viewport was last placed at.
    /// </summary>
    [DataField]
    public MapCoordinates? Selected;

    /// <summary>
    /// Invisible anchored entity at the viewport location; carries the eye used by the client window.
    /// </summary>
    [DataField]
    public EntityUid? ViewportAnchor;

    /// <summary>
    /// Whether the client viewport window is currently open (toggled by the open action).
    /// </summary>
    [DataField]
    public bool IsWindowOpen;

    [DataField]
    public TimeSpan SetCooldown = TimeSpan.FromSeconds(30);

    [DataField]
    public TimeSpan NextSetTime;

    [DataField]
    public Vector2i WindowSize = new(512, 512);

    [DataField]
    public int ZoomLevel = 1;
}
