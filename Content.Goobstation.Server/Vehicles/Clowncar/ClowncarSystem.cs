using System.Linq;
using Content.Goobstation.Shared.Vehicles;
using Content.Goobstation.Shared.Vehicles.Clowncar;
using Content.Server.Chat.Systems;
using Content.Shared.ActionBlocker;
using Content.Shared.Buckle;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Verbs;
using Robust.Shared.Containers;

namespace Content.Goobstation.Server.Vehicles.Clowncar;

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
        SubscribeLocalEvent<ClowncarComponent, ThankRiderActionEvent>(OnThankRider);
        SubscribeLocalEvent<ClowncarComponent, GetVerbsEvent<AlternativeVerb>>(AddVerbs);
        SubscribeLocalEvent<ClowncarComponent, ClownCarEnterDriverSeatDoAfterEvent>(OnEnterDriverSeat);
        SubscribeLocalEvent<ClowncarComponent, ClownCarOpenTrunkDoAfterEvent>(OnOpenTrunk);
        SubscribeLocalEvent<ClowncarComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<ClowncarComponent, QuietBackThereActionEvent>(OnQuietInTheBack);
    }

    private void OnThankRider(EntityUid uid, ClowncarComponent component, ThankRiderActionEvent args)
    {
        if (!TryComp<VehicleComponent>(uid, out var vehicle) || args.Handled)
            return;

        component.ThankCounter++;

        if (vehicle.Driver == null)
        {
            _chatSystem.TrySendInGameICMessage(args.Performer, Loc.GetString("clowncar-thank-no-driver"), InGameICChatType.Speak, false);
            args.Handled = true;

            if (_container.TryGetContainer(uid, component.Container, out var container))
                _container.Remove(args.Performer, container);

            return;
        }

        var message = Loc.GetString("clowncar-thank-driver", ("driver", vehicle.Driver));
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
            verb.Text = "Enter Driver seat";
            verb.Act = () => EnterDriverSeatVerb(uid, verbs.User, component);
            verbs.Verbs.Add(verb);
        }

        AlternativeVerb verb2 = new();
        verb2.Text = "Open Trunk";
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
        OpenTrunk(uid, component);
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

    private void OnExamined(EntityUid uid, ClowncarComponent component, ref ExaminedEvent args)
    {
        if (!_container.TryGetContainer(uid, component.Container, out var container))
            return;

        if (args.IsInDetailsRange)
            args.PushMarkup("Contains: " + container.Count + " Happy Passengers");
    }

    private void OnQuietInTheBack(EntityUid uid, ClowncarComponent component, QuietBackThereActionEvent args)
    {
        component.ThankCounter = 0;
        _chatSystem.TrySendInGameICMessage(args.Performer, Loc.GetString("clowncar-quiet-in-the-back"), InGameICChatType.Speak, false);
    }

}
