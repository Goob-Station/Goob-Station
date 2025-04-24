// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Devil.Contract;

[Prototype("clause")]
public sealed class DevilClausePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private init; } = default!;

    [DataField(required: true)]
    public int ClauseWeight;

    [DataField]
    public ComponentRegistry? AddedComponents;

    [DataField]
    public ComponentRegistry? RemovedComponents;

    [DataField]
    public string? DamageModifierSet;

    [DataField]
    public BaseDevilContractEvent? Event;

}

public enum SpecialCase : byte
{
    SoulOwnership,
    RemoveHand,
    RemoveLeg,
    RemoveOrgan,
}
