// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Shadowling;
using Content.Goobstation.Shared.Shadowling.Components;
using Content.Goobstation.Shared.Shadowling.Components.Abilities.PreAscension;
using Content.Server.DoAfter;
using Content.Server.Popups;
using Content.Shared.DoAfter;
using Content.Shared.Mindshield.Components;
using Content.Shared.Popups;

namespace Content.Goobstation.Server.Shadowling.Systems.Abilities.PreAscension;

/// <summary>
/// This handles the Enthrall Abilities
/// </summary>
public sealed class ShadowlingEnthrallSystem : EntitySystem
{
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly ShadowlingSystem _shadowling = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<ShadowlingEnthrallComponent, EnthrallEvent>(OnEnthrall);
        SubscribeLocalEvent<ShadowlingEnthrallComponent, EnthrallDoAfterEvent>(OnEnthrallDoAfter);
    }

    private void OnEnthrall(EntityUid uid, ShadowlingEnthrallComponent comp, EnthrallEvent args)
    {
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
            _popup.PopupEntity(Loc.GetString("shadowling-enthrall-mindshield"), uid, uid, PopupType.SmallCaution);
            return;
        }

        _popup.PopupEntity(Loc.GetString("shadowling-target-being-thralled"), uid, target, PopupType.SmallCaution);

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnEnthrallDoAfter(EntityUid uid, ShadowlingEnthrallComponent comp, EnthrallDoAfterEvent args)
    {
        _shadowling.DoEnthrall(uid, args);
    }
}
