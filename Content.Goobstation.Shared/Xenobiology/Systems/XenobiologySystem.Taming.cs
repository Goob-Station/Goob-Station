// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Random.Helpers;

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

        var now = _timing.CurTime;
        var interval = ent.Comp.InteractInterval;

        if (now < ent.Comp.LastInteract + interval)
            return;

        ent.Comp.LastInteract = now;

        if (ent.Comp.Tamer != null)
        {
            _popup.PopupPredicted(Loc.GetString("slime-interaction-tame-fail"), user, user);
            return;
        }

        var rand = SharedRandomExtensions.PredictedRandom(_timing, GetNetEntity(ent)); // Don't use _random or it will mispredict

        var min = ent.Comp.MinChance;
        var max = ent.Comp.MaxChance;
        var divider = ent.Comp.SuccessDivider;

        if (Math.Min(min, max) < 0 || min >= max || divider <= 0)
            return;

        if (rand.Next(min, max) > (min + max) / divider)
        {
            _popup.PopupPredicted(Loc.GetString("slime-interaction-tame-failed"), user, user);

            var ev = new SlimeFailedTameEvent(args.User);
            RaiseLocalEvent(ent.Owner, ref ev);

            return;
        }

        var coords = Transform(ent).Coordinates;
        PredictedSpawnAtPosition(ent.Comp.TameEffect, coords);
        ent.Comp.Tamer = user;

        _popup.PopupPredicted(Loc.GetString("slime-interaction-tame", ("slime", ent.Owner), ("tamer", user)), ent.Owner, user);

        Dirty(ent);
    }
}
