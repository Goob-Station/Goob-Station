// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Enchanting.Systems;
using Content.Shared.StepTrigger.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Enchanting.Components;

/// <summary>
/// Provides immunity to step triggers of a certain type, lava by default.
/// Does nothing to other step triggers.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(LavaImmunityEnchantSystem))]
[EntityCategory("Enchants")]
public sealed partial class LavaImmunityEnchantComponent : Component
{
    [DataField]
    public StepTriggerGroup Group = new()
    {
        Types = new List<ProtoId<StepTriggerTypePrototype>>()
        {
            "Lava"
        }
    };
}
