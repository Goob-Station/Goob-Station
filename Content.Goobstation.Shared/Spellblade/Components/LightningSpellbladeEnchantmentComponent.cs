// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Spellblade.Systems;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Spellblade.Components;

[RegisterComponent, Access(typeof(SharedSpellbladeSystem))]
public sealed partial class LightningSpellbladeEnchantmentComponent : Component
{
    [DataField]
    public float ShockDamage = 30f;

    [DataField]
    public float ShockTime = 1f;

    [DataField]
    public float Range = 4f;

    [DataField]
    public int BoltCount = 3;

    [DataField]
    public int ArcDepth = 1;

    [DataField]
    public float Siemens = 1f;

    [DataField]
    public EntProtoId LightningPrototype = "HyperchargedLightning";
}