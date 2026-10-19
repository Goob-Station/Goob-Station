// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;

namespace Content.Shared.MalfAI;

/// <summary>
/// Per-AI state for the camera upgrade.
/// Desired is the player's toggle preference; Effective is whether it's currently active
/// (e.g., only when the AI is in its core).
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MalfAiCameraUpgradeComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool EnabledDesired;

    [DataField, AutoNetworkedField]
    public bool EnabledEffective;
}
