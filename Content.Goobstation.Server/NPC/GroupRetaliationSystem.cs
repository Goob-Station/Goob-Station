// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.NPC.Components;
using Content.Server.NPC.Events;
using Content.Server.NPC.Systems;
using Content.Shared.NPC.Systems;

namespace Content.Goobstation.Server.NPC;

/// <summary>
///     Handles NPC which become aggressive after being attacked.
/// </summary>
public sealed partial class GroupRetaliationSystem : EntitySystem
{
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private NpcFactionSystem _npcFaction = default!;
    [Dependency] private NPCRetaliationSystem _retaliation = default!;

    /// <inheritdoc />
    public override void Initialize()
    {
        SubscribeLocalEvent<GroupRetaliationComponent, NPCRetaliatedEvent>(OnRetaliated);
    }

    private void OnRetaliated(Entity<GroupRetaliationComponent> ent, ref NPCRetaliatedEvent args)
    {
        if (args.Secondary)
            return;

        foreach (var uid in _lookup.GetEntitiesInRange<GroupRetaliationComponent>(Transform(args.Ent).Coordinates, ent.Comp.Range))
        {
            if (!_npcFaction.IsEntityFriendly(ent.Owner, uid.Owner) || !TryComp<NPCRetaliationComponent>(uid, out var npcRetaliation))
                continue;

            _retaliation.TryRetaliate((uid, npcRetaliation), args.Against, true);
        }
    }
}
