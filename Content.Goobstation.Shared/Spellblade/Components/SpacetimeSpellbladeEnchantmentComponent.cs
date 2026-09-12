// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Spellblade.Systems;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Spellblade.Components;

[RegisterComponent, Access(typeof(SharedSpellbladeSystem))]
public sealed partial class SpacetimeSpellbladeEnchantmentComponent : Component
{
    [DataField]
    public EntProtoId Effect = "WeaponArcTempSlash";
}
