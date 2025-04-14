// SPDX-FileCopyrightText: 2025 BeBright <98597725+be1bright@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Content.Shared.Damage.Prototypes;

namespace Content.Shared.Mech.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ArmorPlateComponent : Component
{
    [DataField("damageModifierSet")]
    public ProtoId<DamageModifierSetPrototype>? DamageModifierSetId;
}
