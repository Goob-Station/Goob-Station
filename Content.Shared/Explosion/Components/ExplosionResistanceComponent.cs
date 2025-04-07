// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 beck-thompson <107373427+beck-thompson@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Explosion.EntitySystems;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;
using Robust.Shared.GameStates;

namespace Content.Shared.Explosion.Components;

/// <summary>
/// Component that provides entities with explosion resistance.
/// By default this is applied when worn, but to solely protect the entity itself and
/// not the wearer use <c>worn: false</c>.
/// </summary>
/// <remarks>
///     This is desirable over just using damage modifier sets, given that equipment like bomb-suits need to
///     significantly reduce the damage, but shouldn't be silly overpowered in regular combat.
/// </remarks>
[NetworkedComponent, RegisterComponent]
[Access(typeof(SharedExplosionSystem))]
public sealed partial class ExplosionResistanceComponent : Component
{
    /// <summary>
    ///     The explosive resistance coefficient, This fraction is multiplied into the total resistance.
    /// </summary>
    [DataField("damageCoefficient")]
    public float DamageCoefficient = 1;

    /// <summary>
    /// When true, resistances will be applied to the entity wearing this item.
    /// When false, only this entity will get th resistance.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool Worn = true;

    /// <summary>
    /// Examine string for explosion resistance.
    /// Passed <c>value</c> from 0 to 100.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public LocId Examine = "explosion-resistance-coefficient-value";

    /// <summary>
    ///     Modifiers specific to each explosion type for more customizability.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("modifiers", customTypeSerializer: typeof(PrototypeIdDictionarySerializer<float, ExplosionPrototype>))]
    public Dictionary<string, float> Modifiers = new();
}