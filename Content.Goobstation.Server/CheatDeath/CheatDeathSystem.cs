// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.DelayedDeath;
using Content.Goobstation.Shared.CheatDeath;
using Content.Server._Shitmed.DelayedDeath;
using Content.Server.Actions;
using Content.Server.Administration.Systems;
using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Traits.Assorted;
using Robust.Shared.Network;

namespace Content.Goobstation.Server.CheatDeath;

public sealed partial class CheatDeathSystem : EntitySystem
{
    [Dependency] private readonly RejuvenateSystem _rejuvenateSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly ActionsSystem _actionsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CheatDeathComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<CheatDeathComponent, CheatDeathEvent>(OnDeathCheatAttempt);
        SubscribeLocalEvent<CheatDeathComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<CheatDeathComponent, DelayedDeathEvent>(OnDelayedDeath);
    }

    private void OnStartup(EntityUid uid, CheatDeathComponent comp, ref ComponentStartup args)
    {
        _actionsSystem.AddAction(uid, comp.ActionCheatDeath);
    }

    private void OnExamined(Entity<CheatDeathComponent> ent, ref ExaminedEvent args)
    {
        if (args.Examined == args.Examiner)
            args.PushMarkup(Loc.GetString("cheat-death-component-remaining-revives", ("amount", ent.Comp.ReviveAmount)));
    }

    private void OnDelayedDeath(EntityUid uid, CheatDeathComponent comp, ref DelayedDeathEvent args)
    {
        if (args.Cancelled)
            return;

        RemComp(uid, comp);
    }

    private void OnDeathCheatAttempt(Entity<CheatDeathComponent> ent, ref CheatDeathEvent args)
    {
        if (args.Handled || (!_mobStateSystem.IsDead(ent) && !ent.Comp.CanCheatStanding))
            return;

        // If the entity is out of revives, or if they are unrevivable, return.
        if (ent.Comp.ReviveAmount == 0 || HasComp<UnrevivableComponent>(args.Performer))
        {
            var failPopup = Loc.GetString("action-cheat-death-fail");
            _popupSystem.PopupEntity(failPopup, ent, PopupType.LargeCaution);

            return;
        }

        // Revive entity
        _rejuvenateSystem.PerformRejuvenate(ent);

        // Show popup
        var popup = Loc.GetString("action-cheated-death", ("name", Name(ent)));
        _popupSystem.PopupEntity(popup, ent, PopupType.LargeCaution);

        // Decrement remaining revives.
        if (ent.Comp.ReviveAmount != -1)
            ent.Comp.ReviveAmount--;
        args.Handled = true;

    }
}
