// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Random.Helpers;
using Robust.Shared.Prototypes;

namespace Content.Shared.EntityTable.EntitySelectors;

/// <summary>
/// Gets the spawns from one of the child selectors, based on the weight of the children
/// </summary>
public sealed partial class GroupSelector : EntityTableSelector
{
    [DataField(required: true)]
    public List<EntityTableSelector> Children = new();

    protected override IEnumerable<EntProtoId> GetSpawnsImplementation(System.Random rand,
        IEntityManager entMan,
        IPrototypeManager proto)
    {
        var children = new Dictionary<EntityTableSelector, float>(Children.Count);
        foreach (var child in Children)
        {
            children.Add(child, child.Weight);
        }

        var pick = SharedRandomExtensions.Pick(children, rand);

        return pick.GetSpawns(rand, entMan, proto);
    }
}