// SPDX-FileCopyrightText: 2026 White Dream Project contributors
// SPDX-FileCopyrightText: 2026 v0id <>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._White.RadialSelector;
using Robust.Shared.GameStates;

namespace Content.Shared.BloodCult.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class CultConstructionMenuComponent : Component
{
    [DataField(required: true)]
    public List<RadialSelectorEntry> Entries = new();
}
