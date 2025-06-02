// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Xenobiology;

/// <summary>
/// This prototype stores information about different slime breeds.
/// </summary>
[Prototype]
public sealed partial class BreedPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private init; } = null!;

    /// <summary>
    /// Used to set the slime's name.
    /// </summary>
    [DataField(required: true)]
    public string BreedName = string.Empty;

    /// <summary>
    /// What components should be given to the slime mob? Usually SlimeComponent.
    /// </summary>
    [DataField]
    public ComponentRegistry Components = new();
}
