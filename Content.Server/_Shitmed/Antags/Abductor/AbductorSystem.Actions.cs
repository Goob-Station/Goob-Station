// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Shitmed.Antags.Abductor;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Effects;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Spawners;
using Robust.Shared.Audio.Systems;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Movement.Pulling.Components;

namespace Content.Server._Shitmed.Antags.Abductor;

public sealed partial class AbductorSystem : SharedAbductorSystem
{
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly SharedColorFlashEffectSystem _color = default!;
    [Dependency] private readonly PullingSystem _pullingSystem = default!;

    private static readonly EntProtoId<InstantActionComponent> SendYourself = "ActionSendYourself";
    private static readonly EntProtoId<InstantActionComponent> ExitAction = "ActionExitConsole";
    private static readonly EntProtoId TeleportationEffect = "EffectTeleportation";
    private static readonly EntProtoId TeleportationEffectEntity = "EffectTeleportationEntity";

    public void InitializeActions()
    {
        SubscribeLocalEvent<AbductorScientistComponent, ComponentStartup>(AbductorScientistComponentStartup);

        SubscribeLocalEvent<ExitConsoleEvent>(OnExit);

        SubscribeLocalEvent<AbductorReturnToShipEvent>(OnReturn);
        SubscribeLocalEvent<AbductorScientistComponent, AbductorReturnDoAfterEvent>(OnDoAfterAbductorReturn);

        SubscribeLocalEvent<SendYourselfEvent>(OnSendYourself);
        SubscribeLocalEvent<AbductorScientistComponent, AbductorSendYourselfDoAfterEvent>(OnDoAfterSendYourself);
    }

    private void AbductorScientistComponentStartup(Entity<AbductorScientistComponent> ent, ref ComponentStartup args)
        => ent.Comp.SpawnPosition = EnsureComp<TransformComponent>(ent).Coordinates;

    private void OnReturn(AbductorReturnToShipEvent ev)
    {
        EnsureComp<AbductorScientistComponent>(ev.Performer, out var abductorScientistComponent);
        AddTeleportationEffect(ev.Performer, 3.0f, TeleportationEffectEntity, out var effectEnt, true, true);

        if (abductorScientistComponent.SpawnPosition.HasValue)
        {
            var effect = _entityManager.SpawnEntity(TeleportationEffect, abductorScientistComponent.SpawnPosition.Value);
            EnsureComp<TimedDespawnComponent>(effect, out var despawnComp);
            despawnComp.Lifetime = 3.0f;
            _audioSystem.PlayPvs("/Audio/_Shitmed/Misc/alien_teleport.ogg", effect);
        }

        var doAfter = new DoAfterArgs(EntityManager, ev.Performer, TimeSpan.FromSeconds(3), new AbductorReturnDoAfterEvent(), ev.Performer)
        {
            MultiplyDelay = false,
        };
        _doAfter.TryStartDoAfter(doAfter);
        ev.Handled = true;
    }
    private void OnDoAfterAbductorReturn(Entity<AbductorScientistComponent> ent, ref AbductorReturnDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        _color.RaiseEffect(Color.FromHex("#BA0099"), new List<EntityUid>(1) { ent }, Filter.Pvs(ent, entityManager: EntityManager));
        StopPulls(ent);
        if (ent.Comp.SpawnPosition is not null)
            _xformSys.SetCoordinates(ent, ent.Comp.SpawnPosition.Value);
        OnCameraExit(ent);
    }

    private void OnSendYourself(SendYourselfEvent ev)
    {
        AddTeleportationEffect(ev.Performer, 5.0f, TeleportationEffectEntity, out var effectEnt, true, false);
        var effect = _entityManager.SpawnEntity(TeleportationEffect, ev.Target);
        EnsureComp<TimedDespawnComponent>(effect, out var _);

        var @event = new AbductorSendYourselfDoAfterEvent(GetNetCoordinates(ev.Target));
        var doAfter = new DoAfterArgs(EntityManager, ev.Performer, TimeSpan.FromSeconds(5), @event, ev.Performer);
        _doAfter.TryStartDoAfter(doAfter);
        ev.Handled = true;
    }
    private void OnDoAfterSendYourself(Entity<AbductorScientistComponent> ent, ref AbductorSendYourselfDoAfterEvent args)
    {
        _color.RaiseEffect(Color.FromHex("#BA0099"), new List<EntityUid>(1) { ent }, Filter.Pvs(ent, entityManager: EntityManager));
        StopPulls(ent);
        _xformSys.SetCoordinates(ent, GetCoordinates(args.TargetCoordinates));
        OnCameraExit(ent);
    }

    private void OnExit(ExitConsoleEvent ev) => OnCameraExit(ev.Performer);

    private void AddActions(AbductorBeaconChosenBuiMsg args)
    {
        EnsureComp<AbductorsAbilitiesComponent>(args.Actor, out var comp);
        comp.HiddenActions = _actions.HideActions(args.Actor);
        _actions.AddAction(args.Actor, ref comp.ExitConsole, ExitAction);
        _actions.AddAction(args.Actor, ref comp.SendYourself, SendYourself);
    }
    private void RemoveActions(EntityUid actor)
    {
        EnsureComp<AbductorsAbilitiesComponent>(actor, out var comp);
        _actions.RemoveAction(actor, comp.ExitConsole);
        _actions.RemoveAction(actor, comp.SendYourself);
        _actions.UnHideActions(actor, comp.HiddenActions);
    }

    private void StopPulls(EntityUid ent)
    {
        if (_pullingSystem.IsPulling(ent))
        {
            if (!TryComp<PullerComponent>(ent, out var pullerComp)
                || pullerComp.Pulling == null
                || !TryComp<PullableComponent>(pullerComp.Pulling.Value, out var pullableComp)
                || !_pullingSystem.TryStopPull(pullerComp.Pulling.Value, pullableComp)) return;
        }

        if (_pullingSystem.IsPulled(ent))
        {
            if (!TryComp<PullableComponent>(ent, out var pullableComp)
                || !_pullingSystem.TryStopPull(ent, pullableComp)) return;
        }
    }

    private void AddTeleportationEffect(EntityUid performer,
        float lifetime,
        EntProtoId effectEntity,
        out EntityUid effectEnt,
        bool applyColor = true,
        bool playAudio = true)
    {
        if (applyColor)
            _color.RaiseEffect(Color.FromHex("#BA0099"), new List<EntityUid>(1) { performer }, Filter.Pvs(performer, entityManager: EntityManager));

        EnsureComp<TransformComponent>(performer, out var xform);
        effectEnt = SpawnAttachedTo(effectEntity, xform.Coordinates);
        _xformSys.SetParent(effectEnt, performer);
        EnsureComp<TimedDespawnComponent>(effectEnt, out var despawnComp);
        despawnComp.Lifetime = lifetime;

        if (playAudio)
            _audioSystem.PlayPvs("/Audio/_Shitmed/Misc/alien_teleport.ogg", effectEnt);
    }
}