// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Damage;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;

namespace Content.Goobstation.Server.Devil.Grip;

[RegisterComponent]
public sealed partial class DevilGripComponent : Component
{
    [DataField]
    public TimeSpan CooldownAfterUse = TimeSpan.FromSeconds(20);

    [DataField]
    public EntityWhitelist Blacklist = new();

    [DataField]
    public TimeSpan KnockdownTime = TimeSpan.FromSeconds(3f);

    [DataField]
    public TimeSpan KnockdownTimeIncrement = TimeSpan.FromSeconds(2f);

    [DataField]
    public float StaminaDamage = 80f;

    [DataField]
    public TimeSpan SpeechTime = TimeSpan.FromSeconds(10f);

    [DataField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/_Goobstation/Effects/bone_crack.ogg");

    [DataField]
    public LocId Invocation = "devil-speech-grip";

    // Devil doesn't have a YAML so we add it here.
    [DataField]
    public DamageSpecifier Healing = new DamageSpecifier()
    {
        DamageDict =
        {
            { "Heat", -15 },{ "Slash", -15 },{ "Blunt", -15 },{ "Piercing", -15 }
        }
    };
}
