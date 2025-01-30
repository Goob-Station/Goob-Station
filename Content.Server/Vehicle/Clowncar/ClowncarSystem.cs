using System.Linq;
using Content.Server.Chat.Systems;
using Content.Shared.Buckle.Components;
using Content.Shared.IdentityManagement;
using Content.Shared.Vehicle.Clowncar;
using Content.Shared.Vehicles;
using Content.Shared.ActionBlocker;
using Content.Shared.Verbs;
using Content.Shared.DoAfter;
using Robust.Shared.Containers;
using Content.Shared.Buckle;


namespace Content.Server.Vehicle.Clowncar;

public sealed class ClowncarSystem : SharedClowncarSystem
{
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedBuckleSystem _buckle = default!;


    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        // TODO this is not in shared because we cant send IC message from there
        SubscribeLocalEvent<ClowncarComponent, ThankRiderActionEvent>(OnThankRider);
        SubscribeLocalEvent<ClowncarComponent, GetVerbsEvent<AlternativeVerb>>(AddVerbs);
        SubscribeLocalEvent<ClowncarComponent, ClownCarEnterDriverSeatDoAfterEvent>(OnEnterDriverSeat);
        SubscribeLocalEvent<ClowncarComponent, ClownCarOpenTrunkDoAfterEvent>(OnOpenTrunk);
    }
    private void OnThankRider(EntityUid uid, ClowncarComponent component, ThankRiderActionEvent args)
    {
        if (!TryComp<VehicleComponent>(uid, out var vehicle) || args.Handled)
            return;

        if (vehicle.Driver == null){
            //Todo make popup if no driver
            args.Handled = true;
            return;
        }

        component.ThankCounter++;
        var message = Loc.GetString("clowncar-thankrider", ("rider", vehicle.Driver));
        _chatSystem.TrySendInGameICMessage(args.Performer, message, InGameICChatType.Speak, false);
        args.Handled = true;

        if (component.ThankCounter >= 5)
            OpenTrunk(uid, component);
    }

    private void AddVerbs(EntityUid uid, ClowncarComponent component, GetVerbsEvent<AlternativeVerb> verbs)
    {
        if (!_actionBlocker.CanInteract(verbs.User, uid))
            return;
        if (!_container.TryGetContainer(uid, component.Container, out var container))
            return;
        if (container.Contains(verbs.User))
            return;
        if (!TryComp<VehicleComponent>(uid, out var vehicle))
            return;

        if (vehicle.Driver == null){
            AlternativeVerb verb = new();
            verb.Text = "Enter Driverseat"; //Loc.GetString("surveillance-camera-setup");
            verb.Act = () => EnterDriverSeatVerb(uid, verbs.User, component);
            verbs.Verbs.Add(verb);
        }

        AlternativeVerb verb2 = new();
        verb2.Text = "Open Trunk"; //Loc.GetString("surveillance-camera-setup");
        verb2.Act = () => OpenTrunkVerb(uid, verbs.User, component);
        verbs.Verbs.Add(verb2);
    }
    private void EnterDriverSeatVerb(EntityUid uid, EntityUid player, ClowncarComponent component)
    {
        var doAfterEventArgs =
        new DoAfterArgs(EntityManager, player, 3f, new ClownCarEnterDriverSeatDoAfterEvent(), uid)
        {
            NeedHand = true,
            BreakOnMove = true,
            BreakOnWeightlessMove = false,
        };
        _doAfter.TryStartDoAfter(doAfterEventArgs);

    }
    private void OnEnterDriverSeat(EntityUid uid, ClowncarComponent component, ClownCarEnterDriverSeatDoAfterEvent args)
    {
        if (!_container.TryGetContainer(uid, component.Container, out var container))
            return;
        if (container.Contains(args.User))
            return;
        if (!TryComp<VehicleComponent>(uid, out var vehicle))
            return;
        if (vehicle.Driver != null)
            return;

        _buckle.TryBuckle(args.User, args.User, uid);
    }
    private void OpenTrunkVerb(EntityUid uid, EntityUid player, ClowncarComponent component)
    {
        var doAfterEventArgs =
        new DoAfterArgs(EntityManager, player, 5f, new ClownCarOpenTrunkDoAfterEvent(), uid)
        {
            NeedHand = true,
            BreakOnMove = true,
            BreakOnWeightlessMove = false,
        };
        _doAfter.TryStartDoAfter(doAfterEventArgs);
    }
    private void OnOpenTrunk(EntityUid uid, ClowncarComponent component, ClownCarOpenTrunkDoAfterEvent args)
    {
        if (!_container.TryGetContainer(uid, component.Container, out var container))
            return;
        if (container.Contains(args.User))
            return;
       OpenTrunk(uid,component);
    }
    private void OpenTrunk(EntityUid uid, ClowncarComponent component)
    {
        if (!_container.TryGetContainer(uid, component.Container, out var container))
            return;

        component.ThankCounter = 0;

        foreach (var entity in container.ContainedEntities.ToArray())
        {
            _container.Remove(entity, container);
        }
    }

}
