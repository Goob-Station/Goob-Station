// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SaffronFennec <firefoxwolf2020@protonmail.com>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.CosmicCult.Components;

/// <summary>
/// Component to call back to the cosmic cult ability system regarding a collision
/// </summary>
[NetworkedComponent, RegisterComponent]
public sealed partial class CosmicAstralNovaComponent : Component
{
    [DataField]
    public bool DoStun = true;

    [DataField]
    public DamageSpecifier CosmicNovaDamage = new()
    {
        DamageDict = new() {
            { "Asphyxiation", 5 }
        }
    };
}
