using Content.Server.Actions;
using Content.Server.Chat.Systems;
using Content.Server.Popups;
using Content.Server.RoundEnd;
using Content.Server.Shuttles.Systems;
using Content.Shared._EE.Shadowling;


namespace Content.Server._EE.Shadowling;


/// <summary>
/// This handles Destroy Engines ability.
/// An ability that delays the evacuation shuttle by 10 minutes
/// </summary>
public sealed class ShadowlingDestroyEnginesSystem : EntitySystem
{
    [Dependency] private readonly EmergencyShuttleSystem _emergency = default!;
    [Dependency] private readonly RoundEndSystem _roundEnd = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingDestroyEnginesComponent, DestroyEnginesEvent>(OnDestroyEngines);
    }

    private void OnDestroyEngines(EntityUid uid, ShadowlingDestroyEnginesComponent comp, DestroyEnginesEvent args)
    {
        if (args.Handled)
            return;

        var query = EntityQueryEnumerator<ShadowlingDestroyEnginesComponent>();
        while (query.MoveNext(out _, out var destroyEngines))
        {
            if (destroyEngines.HasBeenUsed)
            {
                _popup.PopupEntity(Loc.GetString("shadowling-destroy-engines-used"), uid);
                return;
            }
        }
        if (_emergency.EmergencyShuttleArrived)
        {
            _popup.PopupEntity(Loc.GetString("shadowling-destroy-engines-arrived"), uid);
            return;
        }

        if (_roundEnd.ExpectedCountdownEnd is null)
        {
            _popup.PopupEntity(Loc.GetString("shadowling-destroy-engines-not-called"), uid);
            return;
        }

        var message = string.Concat(Loc.GetString("shadowling-destroy-engines-message"),
            " ",
            Loc.GetString("shadowling-destroy-engines-delay", ("time", comp.DelayTime.TotalMinutes)));

        _chat.DispatchGlobalAnnouncement(message,
            Loc.GetString("shadowling-destroy-engines-sender"),
            colorOverride: Color.MediumPurple
            );

        // add sound
        comp.HasBeenUsed = true;
        _roundEnd.DelayShuttle(comp.DelayTime);

        _actions.RemoveAction(args.Performer, args.Action);
    }
}
