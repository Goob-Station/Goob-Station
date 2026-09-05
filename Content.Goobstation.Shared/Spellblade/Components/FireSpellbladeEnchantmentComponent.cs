// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Spellblade.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Spellblade.Components;

[RegisterComponent, Access(typeof(SharedSpellbladeSystem))]
public sealed partial class FireSpellbladeEnchantmentComponent : Component
{
    [DataField]
    public float FireStacks = 10f;

    [DataField]
    public float Range = 4f;

    [DataField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/Magic/fireball.ogg");

    [DataField]
    public EntProtoId Effect = "FireFlashEffect";
}