// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.NPC.Components;
using Content.Server.NPC.Events;
using Content.Server.NPC.Systems;
using Content.Shared.NPC.Systems;

namespace Content.Goobstation.Server.NPC;

/// <summary>
///     Handles NPC which become aggressive after being attacked.
/// </summary>
public sealed class GroupRetaliationSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly NpcFactionSystem _npcFaction = default!;
    [Dependency] private readonly NPCRetaliationSystem _retaliation = default!;

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
