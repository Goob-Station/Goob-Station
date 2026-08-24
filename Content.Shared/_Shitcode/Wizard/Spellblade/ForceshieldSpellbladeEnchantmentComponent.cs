// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Goobstation.Wizard.Spellblade;

[RegisterComponent]
public sealed partial class ForceshieldSpellbladeEnchantmentComponent : Component
{
    [DataField]
    public float ShieldLifetime = 5f;
}