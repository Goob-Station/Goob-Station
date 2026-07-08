// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Wizard.Spellblade;

[RegisterComponent]
public sealed partial class SpacetimeSpellbladeEnchantmentComponent : Component
{
    [DataField]
    public EntProtoId Effect = "WeaponArcTempSlash";
}
