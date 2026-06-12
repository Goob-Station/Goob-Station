// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

namespace Content.Shared._Funkystation.MalfAI;

/// <summary>
/// Holds the master lawset that the Malf AI can push to all synced borgs.
/// </summary>
[RegisterComponent]
public sealed partial class MalfMasterLawsetComponent : Component
{
    [DataField]
    public List<string> Laws = new();
}
