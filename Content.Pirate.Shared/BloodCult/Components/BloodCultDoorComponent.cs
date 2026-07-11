// SPDX-FileCopyrightText: 2024 White Dream Project contributors
// SPDX-FileCopyrightText: 2026 v0id <>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared.BloodCult.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class BloodCultDoorComponent : Component
{
    [DataField]
    public EntityWhitelist? Whitelist = new()
    {
        Components = ["BloodCultist", "BloodCultConstruct"],
    };
}
