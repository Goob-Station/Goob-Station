using Content.Shared.Hands;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Systems;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Faction;

public sealed partial class FactionWhileHeldSystem : EntitySystem
{
    [Dependency] private NpcFactionSystem _faction = default!;
    [Dependency] private INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FactionWhileHeldComponent, GotEquippedHandEvent>(OnEquipped);
        SubscribeLocalEvent<FactionWhileHeldComponent, GotUnequippedHandEvent>(OnUnequipped);
    }

    private void OnEquipped(Entity<FactionWhileHeldComponent> ent, ref GotEquippedHandEvent args)
    {
        if (!_net.IsServer)
            return;

        if (!HasComp<NpcFactionMemberComponent>(args.User))
            return;

        if (_faction.IsMember(args.User, ent.Comp.Faction))
            return;

        ent.Comp.Stored = _faction.GetFactions(args.User);

        _faction.ClearFactions(args.User);
        _faction.AddFaction(args.User, ent.Comp.Faction);
    }

    private void OnUnequipped(Entity<FactionWhileHeldComponent> ent, ref GotUnequippedHandEvent args)
    {
        if (!_net.IsServer)
            return;

        if (ent.Comp.Stored is not { } stored)
            return;

        ent.Comp.Stored = null;

        _faction.ClearFactions(args.User);
        _faction.AddFactions(args.User, stored);
    }
}
