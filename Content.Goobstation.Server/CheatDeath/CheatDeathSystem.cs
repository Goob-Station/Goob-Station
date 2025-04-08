// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.CheatDeath;
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
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CheatDeathComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<CheatDeathComponent, CheatDeathEvent>(OnDeathCheatAttempt);
        SubscribeLocalEvent<CheatDeathComponent, ExaminedEvent>(OnExamined);
    }

    private void OnStartup(EntityUid uid, CheatDeathComponent comp, ComponentStartup args)
    {
        _actionsSystem.AddAction(uid, "ActionCheatDeath");
    }

    private void OnExamined(Entity<CheatDeathComponent> comp, ref ExaminedEvent args)
    {
        if (args.Examined == args.Examiner && !_net.IsClient)
            args.PushMarkup(Loc.GetString("cheat-death-component-remaining-revives", ("amount", comp.Comp.ReviveAmount)));
    }

    private void OnDeathCheatAttempt(EntityUid uid, CheatDeathComponent comp, CheatDeathEvent args)
    {
        if (args.Handled)
            return;

        if (!_mobStateSystem.IsDead(uid) && !comp.CanCheatStanding)
            return;

        // If the entity is out of revives, or if they are unrevivable, return.
        if (comp.ReviveAmount == 0 || HasComp<UnrevivableComponent>(args.Performer))
        {
            var failPopup = Loc.GetString("action-cheat-death-fail");
            _popupSystem.PopupEntity(failPopup, uid, PopupType.LargeCaution);

            return;
        }

        // Revive entity
        _rejuvenateSystem.PerformRejuvenate(uid);

        // Show popup
        var popup = Loc.GetString("action-cheated-death", ("name", Name(uid)));
        _popupSystem.PopupEntity(popup, uid, PopupType.LargeCaution);

        // Decrement remaining revives.
        if (comp.ReviveAmount != -1)
            comp.ReviveAmount--;
        args.Handled = true;

    }
}
