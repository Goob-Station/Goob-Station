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
using Content.Server.Jittering;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Traits.Assorted;
using Robust.Shared.Network;

namespace Content.Goobstation.Server.Devil.CheatDeath;

public sealed partial class CheatDeathSystem : EntitySystem
{
    [Dependency] private readonly RejuvenateSystem _rejuvenateSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly ActionsSystem _actionsSystem = default!;
    [Dependency] private readonly JitteringSystem _jitter = default!;
    [Dependency] private readonly MobThresholdSystem _thresholdSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CheatDeathComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<CheatDeathComponent, ComponentRemove>(OnRemoval);

        SubscribeLocalEvent<CheatDeathComponent, CheatDeathEvent>(OnDeathCheatAttempt);

        SubscribeLocalEvent<CheatDeathComponent, ExaminedEvent>(OnExamined);

        SubscribeLocalEvent<CheatDeathComponent, DelayedDeathEvent>(OnDelayedDeath);
    }

    private void OnInit(Entity<CheatDeathComponent> ent, ref MapInitEvent args) =>
        _actionsSystem.AddAction(ent, ref ent.Comp.ActionEntity, ent.Comp.ActionCheatDeath);

    private void OnRemoval(Entity<CheatDeathComponent> ent, ref ComponentRemove args) =>
        _actionsSystem.RemoveAction(ent.Owner, ent.Comp.ActionEntity);

    private void OnExamined(Entity<CheatDeathComponent> ent, ref ExaminedEvent args)
    {
        if (args.Examined != args.Examiner)
            return;

        if (ent.Comp.InfiniteRevives)
        {
            var unlimited = Loc.GetString("cheat-death-component-remaining-revives-unlimited");
            args.PushMarkup(unlimited);

            return;
        }

        var remaining = Loc.GetString("cheat-death-component-remaining-revives", ("amount", ent.Comp.ReviveAmount));
        args.PushMarkup(remaining);

    }

    private void OnDelayedDeath(Entity<CheatDeathComponent> ent, ref DelayedDeathEvent args)
    {
        if (args.PreventRevive)
            RemCompDeferred(ent.Owner, ent.Comp);
    }


    private void OnDeathCheatAttempt(Entity<CheatDeathComponent> ent, ref CheatDeathEvent args)
    {
        if (args.Handled)
            return;

        if (!_mobStateSystem.IsDead(ent) && !ent.Comp.CanCheatStanding)
        {
            var failPopup = Loc.GetString("action-cheat-death-fail-not-dead");
            _popupSystem.PopupEntity(failPopup, ent, ent, PopupType.LargeCaution);

            return;
        }

        // If the entity is out of revives, or if they are unrevivable, return.
        if (ent.Comp.ReviveAmount <= 0 || HasComp<UnrevivableComponent>(ent))
        {
            var failPopup = Loc.GetString("action-cheat-death-fail-no-lives");
            _popupSystem.PopupEntity(failPopup, ent, ent, PopupType.LargeCaution);

            return;
        }

        // If the holy damage exceeds the crit state, do not allow revives.
        if (!TryComp<DamageableComponent>(ent, out var damageable)
            || !_thresholdSystem.TryGetIncapThreshold(ent, out var incapThreshold)
            || damageable.Damage.DamageDict["Holy"] >= incapThreshold)
        {
            var failPopup = Loc.GetString("action-cheat-death-holy-damage");
            _popupSystem.PopupEntity(failPopup, ent, ent, PopupType.LargeCaution);

            return;
        }

        // Show popup
        if (_mobStateSystem.IsDead(ent) && !ent.Comp.CanCheatStanding)
        {
            var popup = Loc.GetString("action-cheated-death-dead", ("name", Name(ent)));
            _popupSystem.PopupEntity(popup, ent, PopupType.LargeCaution);
        }
        else
        {
            var popup = Loc.GetString("action-cheated-death-alive", ("name", Name(ent)));
            _popupSystem.PopupEntity(popup, ent, PopupType.LargeCaution);
        }

        // Revive entity
        _rejuvenateSystem.PerformRejuvenate(ent);
        _jitter.DoJitter(ent, TimeSpan.FromSeconds(5), true);

        // Decrement remaining revives.
        if (!ent.Comp.InfiniteRevives)
            ent.Comp.ReviveAmount--;

        // remove comp if at zero
        if (ent.Comp.ReviveAmount <= 0 && !ent.Comp.InfiniteRevives)
            RemComp(ent.Owner, ent.Comp);

        args.Handled = true;
    }
}
