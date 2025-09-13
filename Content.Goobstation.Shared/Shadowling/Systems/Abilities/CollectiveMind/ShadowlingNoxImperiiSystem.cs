// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.LightDetection.Components;
using Content.Goobstation.Shared.Shadowling.Components;
using Content.Goobstation.Shared.Shadowling.Components.Abilities.CollectiveMind;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Goobstation.Shared.Shadowling.Systems.Abilities.CollectiveMind;

/// <summary>
/// This handles the Nox Imperii system.
/// When used, the shadowling no longer becomes affected by lightning damage.
/// </summary>
public sealed class ShadowlingNoxImperiiSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popups = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingNoxImperiiComponent, NoxImperiiEvent>(OnNoxImperii);
        SubscribeLocalEvent<ShadowlingNoxImperiiComponent, NoxImperiiDoAfterEvent>(OnNoxImperiiDoAfter);
        SubscribeLocalEvent<ShadowlingNoxImperiiComponent, MapInitEvent>(OnStartup);
        SubscribeLocalEvent<ShadowlingNoxImperiiComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<ShadowlingNoxImperiiComponent> ent, ref MapInitEvent args)
        => _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);

    private void OnShutdown(Entity<ShadowlingNoxImperiiComponent> ent, ref ComponentShutdown args)
        => _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);

    private void OnNoxImperii(EntityUid uid, ShadowlingNoxImperiiComponent component, NoxImperiiEvent args)
    {
        if (args.Handled)
            return;

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
        args.Handled = true;
    }

    private void OnNoxImperiiDoAfter(EntityUid uid, ShadowlingNoxImperiiComponent component, NoxImperiiDoAfterEvent args)
    {
        if (args.Handled
            || args.Cancelled
            || !TryComp<ShadowlingComponent>(args.Args.User, out var sling))
            return;

        RemComp<ShadowlingNoxImperiiComponent>(uid);
        RemComp<LightDetectionComponent>(uid);
        RemComp<LightDetectionDamageComponent>(uid);

        // Reduce heat damage from other sources
        sling.HeatDamage.DamageDict["Heat"] = 10;
        sling.HeatDamageProjectileModifier.DamageDict["Heat"] = 4;

        // Indicates that the crew should start caring more since the Shadowling is close to ascension
        _audio.PlayGlobal(new SoundPathSpecifier("/Audio/_EinsteinEngines/Effects/ghost.ogg"), Filter.Broadcast(), false, AudioParams.Default.WithVolume(-4f));
        _popups.PopupPredicted(Loc.GetString("shadowling-nox-imperii-done"), uid, uid, PopupType.Medium);

        args.Handled = true;
    }
}
