// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Prototypes;

namespace Content.Shared.Damage.Prototypes
{
    /// <summary>
    ///     A version of DamageModifierSet that can be serialized as a prototype, but is functionally identical.
    /// </summary>
    /// <remarks>
    ///     Done to avoid removing the 'required' tag on the ID and passing around a 'prototype' when we really
    ///     just want normal data to be deserialized.
    /// </remarks>
    [Prototype("damageModifierSet")]
    public sealed partial class DamageModifierSetPrototype : DamageModifierSet, IPrototype
    {
        [ViewVariables]
        [IdDataField]
        public string ID { get; private set; } = default!;
    }
}