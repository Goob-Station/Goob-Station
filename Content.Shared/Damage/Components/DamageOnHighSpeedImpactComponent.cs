// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 ColdAutumnRain <73938872+ColdAutumnRain@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Silver <silvertorch5@gmail.com>
// SPDX-FileCopyrightText: 2021 Galactic Chimp <GalacticChimpanzee@gmail.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 brainfood1183 <113240905+brainfood1183@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Arendian <137322659+Arendian@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT
using Content.Shared.Damage.Systems;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Damage.Components;

/// <summary>
/// Should the entity take damage / be stunned if colliding at a speed above MinimumSpeed?
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(DamageOnHighSpeedImpactSystem))]
public sealed partial class DamageOnHighSpeedImpactComponent : Component
{
    [DataField("minimumSpeed"), ViewVariables(VVAccess.ReadWrite)]
    public float MinimumSpeed = 20f;

    [DataField("speedDamageFactor"), ViewVariables(VVAccess.ReadWrite)]
    public float SpeedDamageFactor = 0.5f;

    [DataField("soundHit", required: true), ViewVariables(VVAccess.ReadWrite)]
    public SoundSpecifier SoundHit = default!;

    [DataField("stunChance"), ViewVariables(VVAccess.ReadWrite)]
    public float StunChance = 0.25f;

    [DataField("stunMinimumDamage"), ViewVariables(VVAccess.ReadWrite)]
    public int StunMinimumDamage = 10;

    [DataField("stunSeconds"), ViewVariables(VVAccess.ReadWrite)]
    public float StunSeconds = 1f;

    [DataField("damageCooldown"), ViewVariables(VVAccess.ReadWrite)]
    public float DamageCooldown = 2f;

    [DataField("lastHit", customTypeSerializer: typeof(TimeOffsetSerializer)), ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan? LastHit;

    [DataField("damage", required: true), ViewVariables(VVAccess.ReadWrite)]
    public DamageSpecifier Damage = default!;
}