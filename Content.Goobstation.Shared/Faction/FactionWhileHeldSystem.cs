using Content.Goobstation.Shared.CustomFactionIcons;
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

        if (ent.Comp.IconToAdd is { } icon)
        {
            var icons = EnsureComp<CustomFactionIconsComponent>(args.User);

            if (!icons.FactionIcons.Contains(icon))
            {
                icons.FactionIcons.Add(icon);
                ent.Comp.IconAdded = true;
            }
        }
    }

    private void OnUnequipped(Entity<FactionWhileHeldComponent> ent, ref GotUnequippedHandEvent args)
    {
        if (!_net.IsServer)
            return;

        if (ent.Comp.Stored is not { } stored)
            return;

        ent.Comp.Stored = null;

        if (ent.Comp.IconAdded && ent.Comp.IconToAdd is { } icon && TryComp<CustomFactionIconsComponent>(args.User, out var icons))
        {
            icons.FactionIcons.Remove(icon);
        }

        ent.Comp.IconAdded = false;

        _faction.ClearFactions(args.User);
        _faction.AddFactions(args.User, stored);
    }
}
