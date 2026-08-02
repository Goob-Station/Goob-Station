// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Xenobiology;
using Content.Server.NPC.Systems;
using Content.Server.NPC.Components;
using Content.Goobstation.Common.NPC;
using Content.Shared.Popups;

namespace Content.Goobstation.Server.Xenobiology.Systems;

// any other bs needed serverside
public sealed class XenobiologyMiscSystems : EntitySystem
{
    [Dependency] private readonly NPCRetaliationSystem _npc = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<NPCRetaliationComponent, SlimeFailedTameEvent>(OnSlimeFailedTame);
        SubscribeLocalEvent<NPCRetaliationComponent, NPCRetaliatedOverEvent>(OnNPCRetaliatedOver);
    }

    private void OnSlimeFailedTame(Entity<NPCRetaliationComponent> ent, ref SlimeFailedTameEvent args)
    {
        _npc.TryRetaliate(ent, args.Tamer);
    }

    private void OnNPCRetaliatedOver(Entity<NPCRetaliationComponent> ent, ref NPCRetaliatedOverEvent args)
    {
        _popup.PopupEntity(Loc.GetString("npc-retaliation-over", ("entity", ent.Owner), ("target", args.Target)), ent.Owner);
    }
}
