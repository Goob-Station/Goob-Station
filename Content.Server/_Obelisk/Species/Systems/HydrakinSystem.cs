using Content.Server._Obelisk.Species.Components;
using Content.Server.Popups;
using Content.Server.Temperature.Components;
using Content.Server.Temperature.Systems;
using Content.Shared._Mono.Species.Systems;
using Content.Shared.Actions;
using Content.Shared.Actions.Events;
using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Robust.Shared.Audio.Systems;

namespace Content.Server._Obelisk.Species.Systems;

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

        if (!TryComp<TemperatureComponent>(ent, out var temperatureComponent))
            return;

        // Heat capacity equation
        // C_h = Q / dT
        // C_h * dT = Q
        //
        // We want to decrease by CoolOffCoefficient % of the current temperature each ability.
        // E.g, if CoolOffCoefficient is 10%, and you are at 255 degrees you should end at 229.5 degrees.
        // Because this doesn't make any real physical sense, we have to do the math backwards to see how many joules
        // we need to take out to get to the new temperature.

        var dT = -(ent.Comp.CoolOffCoefficient * temperatureComponent.CurrentTemperature);
        var C_h = _temp.GetHeatCapacity(ent);
        var Q = C_h * dT;

        _temp.ChangeHeat(ent, Q, true);

        args.Handled = true;
    }
}
