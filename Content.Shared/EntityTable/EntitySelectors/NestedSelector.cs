// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Robust.Shared.Prototypes;

namespace Content.Shared.EntityTable.EntitySelectors;

/// <summary>
/// Gets the spawns from the entity table prototype specified.
/// Can be used to reuse common tables.
/// </summary>
public sealed partial class NestedSelector : EntityTableSelector
{
    [DataField(required: true)]
    public ProtoId<EntityTablePrototype> TableId;

    protected override IEnumerable<EntProtoId> GetSpawnsImplementation(System.Random rand,
        IEntityManager entMan,
        IPrototypeManager proto)
    {
        return proto.Index(TableId).Table.GetSpawns(rand, entMan, proto);
    }
}