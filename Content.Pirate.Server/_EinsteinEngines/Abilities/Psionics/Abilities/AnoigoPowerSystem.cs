using Content.Shared.Abilities.Psionics;
using Content.Shared.Actions;
using Content.Shared.Actions.Events;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;


namespace Content.Server.Abilities.Psionics;


public sealed class AnoigoPowerSystem : EntitySystem
{
    [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
    [Dependency] private readonly SharedDoorSystem _door = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PsionicComponent, AnoigoPowerActionEvent>(OnPowerUsed);
        SubscribeLocalEvent<DoorComponent, AnoigoEvent>(OnAnoigo);
    }

    private void OnPowerUsed(EntityUid uid, PsionicComponent component, AnoigoPowerActionEvent args)
    {
        if (!_psionics.OnAttemptPowerUse(args.Performer, args.Target, "anoigo", true))
            return;

        var ev = new AnoigoEvent();
        RaiseLocalEvent(args.Target, ev);

        if (!ev.Handled || !_actions.TryGetActionData(args.Action, out var actionData))
            return;
        if (actionData is { UseDelay: not null })
            _actions.SetCooldown(args.Action, actionData.UseDelay.Value / component.CurrentDampening);

        args.Handled = true;
        _psionics.LogPowerUsed(args.Performer, "anoigo");
    }

    private void OnAnoigo(EntityUid target, DoorComponent component, AnoigoEvent args)
    {
        if (TryComp<DoorComponent>(target, out var doorCompWeld) && doorCompWeld.State == DoorState.Welded)
            return;

        if (TryComp<DoorBoltComponent>(target, out var doorBoltComp) && doorBoltComp.BoltsDown)
            _door.SetBoltsDown((target, doorBoltComp), false, predicted: true);

        if (TryComp<DoorComponent>(target, out var doorCompOpen) && doorCompOpen.State is not DoorState.Open)
            _door.StartOpening(target);
        _audio.PlayEntity("/Audio/_EinsteinEngines/Psionics/wavy.ogg", Filter.Pvs(target), target, true);
        args.Handled = true;
    }
    public sealed class AnoigoEvent : HandledEntityEventArgs {}
}
