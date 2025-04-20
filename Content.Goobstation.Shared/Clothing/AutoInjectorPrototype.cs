// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Clothing;

/// <summary>
/// Prototype for clothing-integrated autoinjectors. Could be used for modsuits in the future.
/// </summary>
[Prototype("autoInjector")]
public sealed class AutoInjectorPrototype : IPrototype
{
    /// <summary>
    /// The unique identifier for the prototype.
    /// </summary>
    [IdDataField]
    public string ID { get; private init; } = default!;

    /// <summary>
    /// Dictionary of reagents and their quantities to be injected.
    /// Key: Reagent ID, Value: Quantity to inject.
    /// </summary>
    [DataField(required: true)]
    public Dictionary<string, FixedPoint2> Reagents = new();

    /// <summary>
    /// How long between each injection?
    /// </summary>
    [DataField]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(70);

}
