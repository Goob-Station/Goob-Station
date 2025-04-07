// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Robust.Shared.Prototypes;

namespace Content.Shared.EntityTable.EntitySelectors;

/// <summary>
/// Gets spawns from all of the child selectors
/// </summary>
public sealed partial class AllSelector : EntityTableSelector
{
    [DataField(required: true)]
    public List<EntityTableSelector> Children;

    protected override IEnumerable<EntProtoId> GetSpawnsImplementation(System.Random rand,
        IEntityManager entMan,
        IPrototypeManager proto)
    {
        foreach (var child in Children)
        {
            foreach (var spawn in child.GetSpawns(rand, entMan, proto))
            {
                yield return spawn;
            }
        }
    }
}