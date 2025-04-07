// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 themias <89101928+themias@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Damage.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;

namespace Content.Shared.Damage
{
    /// <summary>
    ///     A set of coefficients or flat modifiers to damage types. Can be applied to <see cref="DamageSpecifier"/> using <see
    ///     cref="DamageSpecifier.ApplyModifierSet(DamageSpecifier, DamageModifierSet)"/>. This can be done several times as the
    ///     <see cref="DamageSpecifier"/> is passed to it's final target. By default the receiving <see cref="DamageableComponent"/>, will
    ///     also apply it's own <see cref="DamageModifierSet"/>.
    /// </summary>
    /// <remarks>
    /// The modifier will only ever be applied to damage that is being dealt. Healing is unmodified.
    /// </remarks>
    [DataDefinition]
    [Serializable, NetSerializable]
    [Virtual]
    public partial class DamageModifierSet
    {
        [DataField("coefficients", customTypeSerializer: typeof(PrototypeIdDictionarySerializer<float, DamageTypePrototype>))]
        public Dictionary<string, float> Coefficients = new();

        [DataField("flatReductions", customTypeSerializer: typeof(PrototypeIdDictionarySerializer<float, DamageTypePrototype>))]
        public Dictionary<string, float> FlatReduction = new();
    }
}