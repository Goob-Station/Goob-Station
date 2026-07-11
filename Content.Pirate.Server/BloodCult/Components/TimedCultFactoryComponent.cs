// SPDX-FileCopyrightText: 2024 Remuchi <72476615+Remuchi@users.noreply.github.com>
// SPDX-FileCopyrightText: 2026 v0id <>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._White.RadialSelector;

namespace Content.Server.BloodCult.Components;

[RegisterComponent]
public sealed partial class TimedCultFactoryComponent : Component
{
    [DataField]
    public bool Active = true;

    [DataField(required: true)]
    public List<RadialSelectorEntry> Entries = new();

    [DataField]
    public float Cooldown = 240f;

    [ViewVariables(VVAccess.ReadOnly)]
    public float CooldownRemaining;
}
