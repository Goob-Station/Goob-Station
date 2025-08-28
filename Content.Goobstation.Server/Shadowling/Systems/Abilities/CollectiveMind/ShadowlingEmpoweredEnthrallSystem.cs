// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Shadowling;
using Content.Goobstation.Shared.Shadowling.Components.Abilities.CollectiveMind;
using Content.Server.Actions;
using Content.Server.DoAfter;
using Content.Server.Popups;
using Content.Shared.DoAfter;
using Content.Shared.Popups;

namespace Content.Goobstation.Server.Shadowling.Systems.Abilities.CollectiveMind;

/// <summary>
/// This handles Empowered Enthrall.
/// Empowered enthrall is like the basic enthrall, however it can bypass Mindshields, has slightly longer range.
/// It also takes less time to enthrall someone.
/// </summary>
public sealed class ShadowlingEmpoweredEnthrallSystem : EntitySystem
{
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly ShadowlingSystem _shadowling = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingEmpoweredEnthrallComponent, EmpoweredEnthrallEvent>(OnEmpEnthrall);
        SubscribeLocalEvent<ShadowlingEmpoweredEnthrallComponent, EmpoweredEnthrallDoAfterEvent>(OnEmpEnthrallDoAfter);
        SubscribeLocalEvent<ShadowlingEmpoweredEnthrallComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<ShadowlingEmpoweredEnthrallComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<ShadowlingEmpoweredEnthrallComponent> ent, ref ComponentStartup args)
        => _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);

    private void OnShutdown(Entity<ShadowlingEmpoweredEnthrallComponent> ent, ref ComponentShutdown args)
        => _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);

    private void OnEmpEnthrall(EntityUid uid, ShadowlingEmpoweredEnthrallComponent component, EmpoweredEnthrallEvent args)
    {
        var target = args.Target;
        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            uid,
            component.EnthrallTime,
            new EmpoweredEnthrallDoAfterEvent(),
            uid,
            target)
        {
            CancelDuplicate = true,
            BreakOnDamage = true,
        };

        if (!_shadowling.CanEnthrall(uid, target))
            return;

        _popup.PopupEntity(Loc.GetString("shadowling-target-being-thralled"), uid, target, PopupType.SmallCaution);

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnEmpEnthrallDoAfter(EntityUid uid, ShadowlingEmpoweredEnthrallComponent component, EmpoweredEnthrallDoAfterEvent args)
    {
        _shadowling.DoEnthrall(uid, args);
    }
}
