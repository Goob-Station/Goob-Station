// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.GameTicking;
using Content.Server.GameTicking;
using Content.Server.Popups;
using Content.Shared.Interaction.Events;

namespace Content.Goobstation.Server.GameTicking;

/// <summary>
/// This system adds a gamerule once an entity uses an item.
/// </summary>
public sealed class AddGameRuleOnUseSystem : EntitySystem
{
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AddGameRuleOnUseComponent, UseInHandEvent>(OnUse);
    }

    private void OnUse(Entity<AddGameRuleOnUseComponent> ent, ref UseInHandEvent args)
    {
        if (ent.Comp.AllowMultipleUses)
        {
            _gameTicker.StartGameRule(ent.Comp.Rule);
            return;
        }

        if (ent.Comp.Used)
        {
            _popupSystem.PopupEntity(Loc.GetString("item-already-added-gamerule"), args.User);
            return;
        }

        var ev = new AddGameRuleItemEvent(args.User);
        RaiseLocalEvent(ent.Owner, ref ev);

        _gameTicker.StartGameRule(ent.Comp.Rule);
        _popupSystem.PopupEntity(Loc.GetString("item-added-gamerule"), args.User);
        ent.Comp.Used = true;
    }
}
