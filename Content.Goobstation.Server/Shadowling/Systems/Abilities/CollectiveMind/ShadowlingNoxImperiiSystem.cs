// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Shadowling;
using Content.Goobstation.Shared.Shadowling.Components;
using Content.Goobstation.Shared.Shadowling.Components.Abilities.CollectiveMind;
using Content.Server.DoAfter;
using Content.Server.Popups;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Shadowling.Systems.Abilities.CollectiveMind;

/// <summary>
/// This handles the Nox Imperii system.
/// When used, the shadowling no longer becomes affected by lightning damage.
/// </summary>
public sealed class ShadowlingNoxImperiiSystem : EntitySystem
{
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly PopupSystem _popups = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingNoxImperiiComponent, NoxImperiiEvent>(OnNoxImperii);
        SubscribeLocalEvent<ShadowlingNoxImperiiComponent, NoxImperiiDoAfterEvent>(OnNoxImperiiDoAfter);
        SubscribeLocalEvent<ShadowlingNoxImperiiComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<ShadowlingNoxImperiiComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<ShadowlingNoxImperiiComponent> ent, ref ComponentStartup args)
        => _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);

    private void OnShutdown(Entity<ShadowlingNoxImperiiComponent> ent, ref ComponentShutdown args)
        => _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);

    private void OnNoxImperii(EntityUid uid, ShadowlingNoxImperiiComponent component, NoxImperiiEvent args)
    {
        var doAfter = new DoAfterArgs(
            EntityManager,
            uid,
            component.Duration,
            new NoxImperiiDoAfterEvent(),
            uid,
            used: args.Action)
        {
            CancelDuplicate = true,
            BreakOnDamage = true,
        };

        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnNoxImperiiDoAfter(EntityUid uid, ShadowlingNoxImperiiComponent component, NoxImperiiDoAfterEvent args)
    {
        if (!TryComp<ShadowlingComponent>(args.Args.User, out var sling))
            return;

        RemComp<ShadowlingNoxImperiiComponent>(uid);
        RemComp<LightDetectionComponent>(uid);
        RemComp<LightDetectionDamageComponent>(uid);

        // Reduce heat damage from other sources
        sling.HeatDamage.DamageDict["Heat"] = 10;
        sling.HeatDamageProjectileModifier.DamageDict["Heat"] = 4;

        _alerts.ClearAlert(uid, sling.AlertProto);

        // Indicates that the crew should start caring more since the Shadowling is close to ascension
        _audio.PlayGlobal(new SoundPathSpecifier("/Audio/_EinsteinEngines/Effects/ghost.ogg"), Filter.Broadcast(), false, AudioParams.Default.WithVolume(-2f));

        _popups.PopupEntity(Loc.GetString("shadowling-nox-imperii-done"), uid, uid, PopupType.Medium);
    }
}
