using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Components.Mobs;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Goobstation.Shared.Wraith.WraithPoints;
using Content.Shared.Flash.Components;
using Content.Shared.Humanoid;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Revenant.Components;
using Content.Shared.StatusEffect;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Goobstation.Shared.Wraith.Systems.Mobs;
public sealed partial class RallySystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RallyComponent, RallyEvent>(OnRally);
    }

    private void OnRally(Entity<RallyComponent> ent, ref RallyEvent args)
    {
        var uid = ent.Owner;
        var comp = ent.Comp;

        if (args.Handled)
            return;

        // Get all entities in range of the rally
        var nearbyEntities = _lookup.GetEntitiesInRange(Transform(uid).Coordinates, comp.RallyRange);

        // Filter to only WraithMinions
        nearbyEntities.RemoveWhere(e => !HasComp<WraithMinionComponent>(e));

        foreach (var minionUid in nearbyEntities)
        {
            //TO DO: Check if they have the component already.
            var rallied = EnsureComp<RalliedComponent>(minionUid);
            _popup.PopupPredicted(Loc.GetString("You wave the flag, rallying the troops!"), uid, uid);
            _popup.PopupPredicted(Loc.GetString("You feel inspired!"), minionUid, uid);

        }
        args.Handled = true;
    }

}
