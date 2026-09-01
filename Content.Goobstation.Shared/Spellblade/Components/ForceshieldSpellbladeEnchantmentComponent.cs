// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Spellblade.Systems;

namespace Content.Goobstation.Shared.Spellblade.Components;

[RegisterComponent, Access(typeof(SharedSpellbladeSystem))]
public sealed partial class ForceshieldSpellbladeEnchantmentComponent : Component
{
    [DataField]
    public float ShieldLifetime = 5f;
}