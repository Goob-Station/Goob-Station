// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-License-Identifier: MIT

using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;

namespace Content.Shared.MalfAI;

/// <summary>
/// Component for the Malf AI "Camera Microphones" upgrade.
/// When enabled and the AI eye is near a microphone-enabled camera,
/// the AI can hear IC chat from entities within voice range of that camera.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MalfAiCameraMicrophonesComponent : Component
{
    /// <summary>
    /// Whether the player wants this upgrade enabled (toggle state).
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool EnabledDesired = false;

    /// <summary>
    /// Whether this upgrade is effectively active right now.
    /// Takes into account both the desired state and core connection status.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool EnabledEffective = false;

    /// <summary>
    /// Maximum distance (in tiles) the AI eye can be from a camera to hear through it.
    /// </summary>
    [DataField]
    public float RadiusTiles = 5.0f;
}
