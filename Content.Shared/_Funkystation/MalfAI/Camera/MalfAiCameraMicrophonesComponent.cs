// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared._Funkystation.MalfAI.Camera;

/// <summary>
/// Added to a Malf AI that has purchased the camera microphones upgrade.
/// Allows hearing chat from players near cameras.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MalfAiCameraMicrophonesComponent : Component
{
    /// <summary>
    /// Whether the upgrade is currently listening. Toggled by the AI's action.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Active = true;

    /// <summary>
    /// Distance the AI eye must be within of a camera for it to relay chat.
    /// </summary>
    [DataField]
    public float RadiusTiles = 6.0f;
}
