// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared._Funkystation.MalfAI;

/// <summary>
/// Added to a Malf AI that has purchased the camera upgrade (x-ray vision near cameras).
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MalfAiCameraUpgradeComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool EnabledDesired;

    [DataField, AutoNetworkedField]
    public bool EnabledEffective;
}
