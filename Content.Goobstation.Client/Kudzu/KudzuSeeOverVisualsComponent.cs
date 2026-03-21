// SPDX-FileCopyrightText: 2026 Raze500
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Client.Kudzu;

/// <summary>
/// Overrides kudzu draw depth for clients that are currently attached to a viewer with <see cref="Shared.Kudzu.SeeOverKudzuComponent" />.
/// </summary>
[RegisterComponent]
public sealed partial class KudzuSeeOverVisualsComponent : Component
{
    [DataField]
    public int NormalDrawDepth = (int) Content.Shared.DrawDepth.DrawDepth.Overdoors;

    [DataField]
    public int SeeOverDrawDepth = (int) Content.Shared.DrawDepth.DrawDepth.HighFloorObjects;
}
