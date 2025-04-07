// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 brainfood1183 <113240905+brainfood1183@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Damage;
using Robust.Shared.Audio;

namespace Content.Server.Weapons.Melee.WeaponRandom;

[RegisterComponent]
internal sealed partial class WeaponRandomComponent : Component
{

    /// <summary>
    /// Amount of damage that will be caused. This is specified in the yaml.
    /// </summary>
    [DataField("damageBonus")]
    public DamageSpecifier DamageBonus = new();

    /// <summary>
    /// Chance for the damage bonus to occur (1 = 100%).
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public float RandomDamageChance = 0.00001f;

    /// <summary>
    /// Sound effect to play when the damage bonus occurs.
    /// </summary>
    [DataField("damageSound")]
    public SoundSpecifier DamageSound = new SoundPathSpecifier("/Audio/Items/bikehorn.ogg");

}