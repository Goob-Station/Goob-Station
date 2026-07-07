// SPDX-FileCopyrightText: 2026 Jonikibaka <153797633+Jonikibaka@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameObjects;

namespace Content.Goobstation.Shared.MalfunctionAi;

/// <summary>
/// Present on an AI that bought Camera Microphones; lets it hear speech near watched cameras.
/// </summary>
[RegisterComponent]
public sealed partial class MalfCameraMicsComponent : Component
{
    /// <summary>How far from a camera the AI's eye must be for that camera's microphone to relay speech.</summary>
    [DataField] public float EyeRange = 4f;

    /// <summary>How far cameras can hear once the upgrade is bought.</summary>
    [DataField] public float ListenRange = 8f;
}
