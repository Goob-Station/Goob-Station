// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Polymorph;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Devil.Contract;

[Prototype("clause")]
public sealed partial class DevilClausePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

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

    [DataField]
    public List<EntProtoId>? Implants;

    [DataField]
    public List<EntProtoId>? SpawnedItems;

    [DataField]
    public ProtoId<PolymorphPrototype>? Polymorph;

}

public enum SpecialCase : byte
{
    SoulOwnership,
    RemoveHand,
    RemoveLeg,
    RemoveOrgan,
}
