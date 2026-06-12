// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

namespace Content.Server._Funkystation.MalfAI;

/// <summary>
/// Marker component placed on a borg that is currently synced to a Malf AI's master lawset.
/// When the Malf AI updates its master lawset, this borg's laws are updated too.
/// </summary>
[RegisterComponent]
public sealed partial class MalfBorgSyncToMasterComponent : Component
{
    [DataField]
    public EntityUid MalfAi;
}
