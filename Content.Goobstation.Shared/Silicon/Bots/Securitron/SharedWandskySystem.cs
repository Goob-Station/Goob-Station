using Content.Goobstation.Common.Silicon.Bots;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Silicon.Bots.Securitron;

/// <summary>
/// This handles system handles Commander Beepsky's actions.
/// </summary>
public sealed class SharedWandskySystem : EntitySystem
{

    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlaveComponent, InteractUsingEvent>(OnGetSlave);

        SubscribeLocalEvent<CommanderComponent, TogglePatrolActionEvent>(OnTogglePatrol);
        SubscribeLocalEvent<CommanderComponent, WaypointActionEvent>(OnWaypointAction);
        SubscribeLocalEvent<CommanderComponent, ClearWaypointsActionEvent>(OnClearWaypoints);
    }

    #region CommanderEvents

    public void OnGetSlave(Entity<SlaveComponent> ent, ref InteractUsingEvent args)
    {
        if (!_net.IsServer)
            return;

        if (!TryComp<CommanderComponent>(args.Used, out var commander))
        {
            return;
        }

        if (ent.Comp.MasterEntity is not null || commander.SlaveEntity is not null)
        {
            _popupSystem.PopupEntity("A bond has already been formed.", ent.Owner, args.Used, PopupType.Medium);
            return;
        }

        var slaveEntity = ent.Owner;

        commander.SlaveEntity = slaveEntity;
        ent.Comp.MasterEntity = ent.Owner;

        Dirty(ent);
        Dirty(args.Used, commander);

        _audio.PlayPvs(commander.EnslaveSound, ent);
    }

    public void OnTogglePatrol(Entity<CommanderComponent> ent, ref TogglePatrolActionEvent args)
    {
        if (!_net.IsServer)
            return;

        if (ent.Comp.SlaveEntity is null
            || !TryComp<SlaveComponent>(ent.Comp.SlaveEntity, out var slave))
        {
            _popupSystem.PopupEntity("You have not synced to a Securitron", ent.Owner, args.Performer, PopupType.Medium);
            return;
        }

        slave.IsPatrolling = !slave.IsPatrolling;

        var message = slave.IsPatrolling
            ? "PATROL ENABLED!"
            : "PATROL DISABLED!";

        Dirty(ent);
        _popupSystem.PopupEntity(message, ent.Owner, args.Performer, PopupType.Medium);
    }

    public void OnWaypointAction(Entity<CommanderComponent> ent, ref WaypointActionEvent args)
    {
        if (!_net.IsServer)
            return;

        if (args.Entity is {} targetEntity
            && HasComp<WaypointComponent>(targetEntity)) // Does not currently work...
        {
            _popupSystem.PopupEntity("Waypoint removed!", args.Performer, args.Performer, PopupType.Medium);
            ent.Comp.Waypoints.Remove(targetEntity);
            QueueDel(targetEntity);
        }
        else if (args.Coords is not null)
        {
            var waypointEntity = Spawn(ent.Comp.WaypointEntityUid, args.Coords.Value);
            ent.Comp.Waypoints.Add(waypointEntity);
            _popupSystem.PopupEntity("Waypoint added!", args.Performer, args.Performer, PopupType.Medium);
        }

        Dirty(ent);
    }

    public void OnClearWaypoints(Entity<CommanderComponent> ent, ref ClearWaypointsActionEvent args)
    {
        var waypoints = ent.Comp.Waypoints;
        var count = waypoints.Count;

        if (!_net.IsServer)
            return;

        if (count == 0)
        {
            _popupSystem.PopupEntity("No waypoints to clear!", ent.Owner, args.Performer, PopupType.Medium);
            return;
        }

        waypoints.RemoveWhere(waypoint =>
        {
            if (!Exists(waypoint))
                return false;
            QueueDel(waypoint);
            return true;
        });

        Dirty(ent);
        _popupSystem.PopupEntity($"Cleared {count} waypoints!", ent.Owner, args.Performer, PopupType.Medium);
    }
    #endregion
}

