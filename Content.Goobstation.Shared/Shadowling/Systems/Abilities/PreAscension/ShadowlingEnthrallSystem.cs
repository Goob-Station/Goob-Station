// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Shadowling.Components;
using Content.Goobstation.Shared.Shadowling.Components.Abilities.PreAscension;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Mindshield.Components;
using Content.Shared.Popups;

namespace Content.Goobstation.Shared.Shadowling.Systems.Abilities.PreAscension;

/// <summary>
/// This handles the Enthrall Abilities
/// </summary>
public sealed class ShadowlingEnthrallSystem : EntitySystem
{
    [Dependency] private readonly SharedShadowlingSystem _shadowling = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ShadowlingEnthrallComponent, EnthrallEvent>(OnEnthrall);
        SubscribeLocalEvent<ShadowlingEnthrallComponent, EnthrallDoAfterEvent>(OnEnthrallDoAfter);
        SubscribeLocalEvent<ShadowlingEnthrallComponent, MapInitEvent>(OnStartup);
        SubscribeLocalEvent<ShadowlingEnthrallComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<ShadowlingEnthrallComponent> ent, ref MapInitEvent args)
        => _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);

    private void OnShutdown(Entity<ShadowlingEnthrallComponent> ent, ref ComponentShutdown args)
        => _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);

    private void OnEnthrall(EntityUid uid, ShadowlingEnthrallComponent comp, EnthrallEvent args)
    {
        if (args.Handled)
            return;

        var target = args.Target;
        var time = comp.EnthrallTime;

        if (TryComp<EnthrallResistanceComponent>(target, out var enthrallRes))
            time += enthrallRes.ExtraTime;

        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            uid,
            time,
            new EnthrallDoAfterEvent(),
            uid,
            target)
        {
            CancelDuplicate = true,
            BreakOnDamage = true,
        };

        if (!_shadowling.CanEnthrall(uid, target))
            return;

        // Basic Enthrall -> Can't melt Mindshields
        if (HasComp<MindShieldComponent>(target))
        {
            _popup.PopupPredicted(Loc.GetString("shadowling-enthrall-mindshield"), uid, uid, PopupType.SmallCaution);
            return;
        }

        _popup.PopupEntity(Loc.GetString("shadowling-target-being-thralled"), target, target, PopupType.LargeCaution);

        _doAfter.TryStartDoAfter(doAfterArgs);
        args.Handled = true;
    }

    private void OnEnthrallDoAfter(EntityUid uid, ShadowlingEnthrallComponent comp, EnthrallDoAfterEvent args)
    {
        if (args.Handled
            || args.Cancelled)
            return;

        _shadowling.DoEnthrall(uid, comp.EnthrallComponents, args);
        args.Handled = true;
    }
}
