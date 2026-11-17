// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Robust.Shared.Audio;

namespace Content.Goobstation.Server.Traps;

[RegisterComponent]
public sealed partial class StepTrapComponent : Component
{
    [DataField]
    public float StaminaDamage = 40f;

    [DataField]
    public TimeSpan SlowdownDuration = TimeSpan.FromSeconds(3f);

    [DataField]
    public float SlowdownModifier = 0.5f;

    [DataField]
    public DamageSpecifier? Damage;

    [DataField]
    public SoundSpecifier? Sound;

    [DataField]
    public float Cooldown = 1f;

    public TimeSpan NextTriggerAt;
}
