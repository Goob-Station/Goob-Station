// SPDX-FileCopyrightText: 2025 sneb
//
// SPDX-License-Identifier: MPL-2.0

using Content.Server.Actions;
using Content.Server.Popups;
using Content.Server.Spawners.Components;
using Content.Server.Species.Systems.Components;
using Content.Server.Temperature.Components;
using Content.Server.Temperature.Systems;
using Content.Shared._Mono.Species.Systems;
using Content.Shared.Actions;
using Content.Shared.Actions.Events;
using Content.Shared.Audio;
using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Temperature.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Serialization;

namespace Content.Server.Species.Systems;

public sealed class HydrakinSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly TemperatureSystem _temp = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HydrakinComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<HydrakinComponent, HydrakinCoolOffActionEvent>(OnCoolOff);
        SubscribeLocalEvent<HydrakinComponent, CoolOffDoAfterEvent>(OnCoolOffDoAfter);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<HydrakinComponent, TemperatureComponent>();
        while (query.MoveNext(out var uid, out var comp, out var temperature))
        {
            if (comp.CurrentTemperatureCooldown > TimeSpan.Zero)
            {
                comp.CurrentTemperatureCooldown -= TimeSpan.FromSeconds(frameTime);
                return;
            }

            if (!comp.HeatBuildupEnabled)
                continue;

            if (TryComp<MobStateComponent>(uid, out var mobState) &&
                mobState.CurrentState != MobState.Alive)
                return;

            if (temperature.CurrentTemperature < comp.MinTemperature ||
                temperature.CurrentTemperature > comp.MaxTemperature)
                return;

            _temp.ChangeHeat(uid,comp.Buildup * comp.TemperatureProcessingCooldown * _temp.GetHeatCapacity(uid), true);
            comp.CurrentTemperatureCooldown = TimeSpan.FromSeconds(comp.TemperatureProcessingCooldown);
        }
    }

    private void OnInit(EntityUid uid, HydrakinComponent component, ComponentInit args)
    {
        if (component.CoolOffAction != null)
            return;

        _actionsSystem.AddAction(uid, ref component.CoolOffAction, component.CoolOffActionId);
    }

    private void OnCoolOff(EntityUid uid, HydrakinComponent component, HydrakinCoolOffActionEvent args)
    {
        var doafter = new DoAfterArgs(EntityManager, uid, TimeSpan.FromSeconds(3), new CoolOffDoAfterEvent(), uid);

        if (!_doAfter.TryStartDoAfter(doafter))
            return;

        args.Handled = true;
    }

    private void OnCoolOffDoAfter(Entity<HydrakinComponent> ent, ref CoolOffDoAfterEvent args)
    {
        _popupSystem.PopupEntity(Loc.GetString("hydrakin-cool-off-emote", ("name", Identity.Entity(ent, EntityManager))), ent);
        _audio.PlayEntity(ent.Comp.CoolOffSound, ent, ent);

        if (!TryComp<TemperatureComponent>(ent, out var temp))
            return;

        _temp.ChangeHeat(ent,
            (temp.CurrentTemperature * ent.Comp.CoolOffCoefficient - temp.CurrentTemperature) * _temp.GetHeatCapacity(ent),
            true);

        args.Handled = true;
    }
}
