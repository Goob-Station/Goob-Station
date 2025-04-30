// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Materials;
using Content.Shared.Power.Generator;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Power.Generator;

/// <summary>
/// Fuels a <see cref="FuelGeneratorComponent"/> through solid materials.
/// </summary>
/// <remarks>
/// <para>
/// Must be accompanied with a <see cref="MaterialStorageComponent"/> to store the actual material and handle insertion logic.
/// You should set a whitelist there for the fuel material.
/// </para>
/// <para>
/// The component itself stores a "fractional" fuel value to allow stack materials to be gradually consumed.
/// </para>
/// </remarks>
[RegisterComponent, Access(typeof(GeneratorSystem))]
public sealed partial class SolidFuelGeneratorAdapterComponent : Component
{
    /// <summary>
    /// The material to accept as fuel.
    /// </summary>
    [DataField("fuelMaterial", customTypeSerializer: typeof(PrototypeIdSerializer<MaterialPrototype>))]
    [ViewVariables(VVAccess.ReadWrite)]
    public string FuelMaterial = "Plasma";

    /// <summary>
    /// How much material (can be fractional) is left in the generator.
    /// </summary>
    [DataField("fractionalMaterial"), ViewVariables(VVAccess.ReadWrite)]
    public float FractionalMaterial;

    /// <summary>
    /// Value to multiply material amount by to get fuel amount.
    /// </summary>
    [DataField("multiplier"), ViewVariables(VVAccess.ReadWrite)]
    public float Multiplier;
}