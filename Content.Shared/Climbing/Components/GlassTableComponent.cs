// SPDX-FileCopyrightText: 2022 Absolute-Potato <jamesgamesmahar@gmail.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Damage;

namespace Content.Shared.Climbing.Components;

/// <summary>
///     Glass tables shatter and stun you when climbed on.
///     This is a really entity-specific behavior, so opted to make it
///     not very generalized with regards to naming.
/// </summary>
[RegisterComponent, Access(typeof(Systems.ClimbSystem))]
public sealed partial class GlassTableComponent : Component
{
    /// <summary>
    ///     How much damage should be given to the climber?
    /// </summary>
    [DataField("climberDamage")]
    public DamageSpecifier ClimberDamage = default!;

    /// <summary>
    ///     How much damage should be given to the table when climbed on?
    /// </summary>
    [DataField("tableDamage")]
    public DamageSpecifier TableDamage = default!;

    /// <summary>
    ///     How much mass should be needed to break the table?
    /// </summary>
    [DataField("tableMassLimit")]
    public float MassLimit;

    /// <summary>
    ///     How long should someone who climbs on this table be stunned for?
    /// </summary>
    public float StunTime = 2.0f;
}