// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Content.Goobstation.Shared.Bible;

namespace Content.Goobstation.Shared.Religion.HonkMotherBaptism;

public sealed partial class HonkmotherBaptismSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HonkmotherBaptismComponent, GetItemActionsEvent>(OnGetItemActions);
    }

    private void OnGetItemActions(Entity<HonkmotherBaptismComponent> ent, ref GetItemActionsEvent args)
    {
        if (!args.InHands || !HasComp<BibleUserComponent>(args.User))
            return;

        args.AddAction(ref ent.Comp.BananaTouchActionEntity, ent.Comp.BananaTouchAction);
    }

}

