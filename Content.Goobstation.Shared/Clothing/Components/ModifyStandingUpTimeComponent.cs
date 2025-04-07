// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Inventory;

namespace Content.Goobstation.Shared.Clothing.Components;

[RegisterComponent]
public sealed partial class ModifyStandingUpTimeComponent : Component
{
    [DataField]
    public float Multiplier = 1f;
}