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

    [SubscribeLocalEvent]
    private void OnEquipped(Entity<FactionWhileHeldComponent> ent, ref GotEquippedHandEvent args)
    {
        if (!_net.IsServer)
            return;

        if (!HasComp<NpcFactionMemberComponent>(args.User))
            return;

        if (_faction.IsMember(args.User, ent.Comp.Faction))
            return;

        var factionOverride = EnsureComp<FactionOverrideComponent>(args.User);
        factionOverride.Factions.Clear();
        factionOverride.Factions.Add(ent.Comp.Faction);

        ent.Comp.Applied = true;

        if (ent.Comp.IconToAdd is { } icon)
        {
            var icons = EnsureComp<CustomFactionIconsComponent>(args.User);

            if (!icons.FactionIcons.Contains(icon))
            {
                icons.FactionIcons.Add(icon);
                Dirty(args.User, icons);
                ent.Comp.IconAdded = true;
            }
        }
    }

    [SubscribeLocalEvent]
    private void OnUnequipped(Entity<FactionWhileHeldComponent> ent, ref GotUnequippedHandEvent args)
    {
        if (!_net.IsServer)
            return;

        if (!ent.Comp.Applied)
            return;

        ent.Comp.Applied = false;

        RemCompDeferred<FactionOverrideComponent>(args.User);

        if (ent.Comp.IconAdded && ent.Comp.IconToAdd is { } icon && TryComp<CustomFactionIconsComponent>(args.User, out var icons))
        {
            icons.FactionIcons.Remove(icon);
            Dirty(args.User, icons);
        }

        ent.Comp.IconAdded = false;
    }
}
