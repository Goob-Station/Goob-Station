// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Shared.Interaction;

namespace Content.Goobstation.Shared.Xenobiology.Systems;

// This handles slime taming, likely to be expanded in the future.
public partial class XenobiologySystem
{
    private void SubscribeTaming()
    {
        SubscribeLocalEvent<SlimeComponent, InteractHandEvent>(OnInteractHand);
    }

    private void OnInteractHand(Entity<SlimeComponent> ent, ref InteractHandEvent args)
    {
        var user = args.User;
        if (ent.Comp.Tamer != null)
        {
            _popup.PopupPredicted(Loc.GetString("slime-interaction-tame-fail"), user, user);
            return;
        }

        var coords = Transform(ent).Coordinates;
        PredictedSpawnAtPosition(ent.Comp.TameEffect, coords);
        ent.Comp.Tamer = user;

        _popup.PopupPredicted(Loc.GetString("slime-interaction-tame"), user, user);
        Dirty(ent);
    }
}
