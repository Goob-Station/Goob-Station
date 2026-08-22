// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared._Trauma.Ranching.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ChangeDamageModiferSetStatusEffectComponent : Component
{
    [DataField]
    public ProtoId<DamageModifierSetPrototype> DamageModifierSet = new ("DevilDealNegative");

    [DataField]
    public ProtoId<DamageModifierSetPrototype>? OriginalDamageModifierSet;

    [DataField]
    public bool GoToOriginalOnRemove = true;
}
