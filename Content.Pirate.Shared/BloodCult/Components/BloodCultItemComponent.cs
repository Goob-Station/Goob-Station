// SPDX-FileCopyrightText: 2024 Remuchi <72476615+Remuchi@users.noreply.github.com>
// SPDX-FileCopyrightText: 2026 v0id <>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Whitelist;

namespace Content.Shared.BloodCult.Components;

[RegisterComponent]
public sealed partial class BloodCultItemComponent : Component
{
    [DataField]
    public EntityWhitelist? Whitelist = new()
    {
        Components = ["BloodCultist", "BloodCultConstruct", "Ghost"],
    };

    [DataField]
    public TimeSpan KnockdownDuration = TimeSpan.FromSeconds(2);
}
