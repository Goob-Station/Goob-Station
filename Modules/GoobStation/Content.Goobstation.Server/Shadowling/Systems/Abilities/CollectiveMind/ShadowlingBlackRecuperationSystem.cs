// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.LightDetection;
using Content.Goobstation.Shared.LightDetection.Components;
using Content.Goobstation.Shared.Shadowling;
using Content.Goobstation.Shared.Shadowling.Components;
using Content.Goobstation.Shared.Shadowling.Components.Abilities.CollectiveMind;
using Content.Server.Administration.Systems;
using Content.Server.EUI;
using Content.Server.Ghost;
using Content.Server.Polymorph.Systems;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.Mind;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Shadowling.Systems.Abilities.CollectiveMind;

/// <summary>
/// This handles the Black Recuperation logic.
/// Black Rec. either turns back a dead Thrall to life, OR turns a living Thrall into a Lesser Shadowling by empowering them
/// Reduces your light resistance forever. Less for thralls, more for lesser shadowlings.
/// </summary>
public sealed class ShadowlingBlackRecuperationSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoidAppearance = default!;
    [Dependency] private readonly LightDetectionDamageSystem _light = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly RejuvenateSystem _rejuvenate = default!;
    [Dependency] private readonly ISharedPlayerManager _playerMan = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly EuiManager _euiManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingBlackRecuperationComponent, BlackRecuperationEvent>(OnBlackRec);
        SubscribeLocalEvent<ShadowlingBlackRecuperationComponent, BlackRecuperationDoAfterEvent>(OnBlackRecDoAfter);
        SubscribeLocalEvent<ShadowlingBlackRecuperationComponent, MapInitEvent>(OnStartup);
        SubscribeLocalEvent<ShadowlingBlackRecuperationComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<ShadowlingBlackRecuperationComponent> ent, ref MapInitEvent args)
        => _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);

    private void OnShutdown(Entity<ShadowlingBlackRecuperationComponent> ent, ref ComponentShutdown args)
        => _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);

    private void OnBlackRec(EntityUid uid, ShadowlingBlackRecuperationComponent component, BlackRecuperationEvent args)
    {
        if (args.Handled)
            return;

        var target = args.Target;

        if (!HasComp<ThrallComponent>(target))
            return;

        if (_mobStateSystem.IsAlive(target) && HasComp<LesserShadowlingComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("shadowling-black-rec-lesser-already"), uid, uid, PopupType.MediumCaution);
            return;
        }

        var doAfter = new DoAfterArgs(
            EntityManager,
            uid,
            component.Duration,
            new BlackRecuperationDoAfterEvent(),
            uid,
            target);

        _doAfter.TryStartDoAfter(doAfter);
        args.Handled = true;
    }

    private void OnBlackRecDoAfter(EntityUid uid, ShadowlingBlackRecuperationComponent component, BlackRecuperationDoAfterEvent args)
    {
        if (args.Cancelled
            || args.Handled
            || args.Target == null)
            return;

        var target = args.Target.Value;

        if (!_mobStateSystem.IsAlive(target))
        {
            if (_mind.TryGetMind(target, out _, out var mind)
                && _playerMan.TryGetSessionById(mind.UserId, out var session))
            {
                // notify them they're being revived.
                if (mind.CurrentEntity != target)
                    _euiManager.OpenEui(new ReturnToBodyEui(mind, _mind, _playerMan), session);
            }
            else
            {
                _popup.PopupEntity(Loc.GetString("defibrillator-no-mind"), uid, uid, PopupType.MediumCaution);
                return;
            }

            _rejuvenate.PerformRejuvenate(target);
            _popup.PopupEntity(Loc.GetString("shadowling-black-rec-revive-done"), uid, target, PopupType.MediumCaution);

            Spawn(component.BlackRecuperationEffect, Transform(target).Coordinates);
            _audio.PlayPvs(component.BlackRecSound, target, AudioParams.Default.WithVolume(-1f));

            if (TryComp<LightDetectionDamageComponent>(uid, out var lightDetectionDamageModifier))
                _light.AddResistance((uid, lightDetectionDamageModifier), component.ResistanceRemoveFromThralls);
        }
        else
        {
            if (component.LesserShadowlingAmount >= component.LesserShadowlingMaxLimit)
            {
                _popup.PopupEntity(Loc.GetString("shadowling-black-rec-limit"), uid, uid, PopupType.MediumCaution);
                return;
            }

            var newUid = _polymorph.PolymorphEntity(target, component.LesserShadowlingSpeciesProto);
            if (newUid == null)
                return;

            var comps = _protoMan.Index(component.LesserSlingComponents);
            EntityManager.AddComponents(newUid.Value, comps);

            if (TryComp<HumanoidAppearanceComponent>(newUid.Value, out var human))
                _humanoidAppearance.AddMarking(newUid.Value, component.MarkingId, Color.Red, true, true, human);

            Spawn(component.BlackRecuperationEffect, Transform(newUid.Value).Coordinates);

            component.LesserShadowlingAmount++;

            _popup.PopupEntity(
                Loc.GetString("shadowling-black-rec-lesser-done"),
                uid,
                newUid.Value,
                PopupType.MediumCaution);
            _audio.PlayPvs(component.BlackRecSound, newUid.Value, AudioParams.Default.WithVolume(-1f));

            if (TryComp<LightDetectionDamageComponent>(uid, out var lightDetectionDamageModifier))
                _light.AddResistance((uid, lightDetectionDamageModifier), component.ResistanceRemoveFromLesser);
        }

        args.Handled = true;
    }
}
