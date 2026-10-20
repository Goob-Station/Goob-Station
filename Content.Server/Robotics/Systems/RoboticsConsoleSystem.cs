// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration.Logs;
using Content.Server.DeviceNetwork.Systems;
using Content.Server.Radio.EntitySystems;
using Content.Server.Silicons.StationAi;
using Content.Shared.Lock;
using Content.Shared.Database;
using Content.Shared.DeviceNetwork;
using Content.Shared.MalfAI;
using Content.Shared.Mind.Components;
using Content.Shared.Robotics;
using Content.Shared.Robotics.Components;
using Content.Shared.Robotics.Systems;
using Content.Shared.Silicons.StationAi;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;
using Content.Shared.DeviceNetwork.Events;

namespace Content.Server.Research.Systems;

/// <summary>
/// Handles UI and state receiving for the robotics control console.
/// <c>BorgTransponderComponent<c/> broadcasts state from the station's borgs to consoles.
/// </summary>
public sealed class RoboticsConsoleSystem : SharedRoboticsConsoleSystem
{
    [Dependency] private readonly DeviceNetworkSystem _deviceNetwork = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly LockSystem _lock = default!;
    [Dependency] private readonly RadioSystem _radio = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly StationAiSystem _stationAi = default!;

    // almost never timing out more than 1 per tick so initialize with that capacity
    private List<string> _removing = new(1);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoboticsConsoleComponent, DeviceNetworkPacketEvent>(OnPacketReceived);
        Subs.BuiEvents<RoboticsConsoleComponent>(RoboticsConsoleUiKey.Key, subs =>
        {
            subs.Event<BoundUIOpenedEvent>(OnOpened);
            subs.Event<RoboticsConsoleDisableMessage>(OnDisable);
            subs.Event<RoboticsConsoleDestroyMessage>(OnDestroy);
            subs.Event<RoboticsConsoleHackMessage>(OnHack);
            // TODO: camera stuff
        });
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<RoboticsConsoleComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            // remove cyborgs that havent pinged in a while
            _removing.Clear();
            foreach (var (address, data) in comp.Cyborgs)
            {
                if (now >= data.Timeout)
                    _removing.Add(address);
            }

            // needed to prevent modifying while iterating it
            foreach (var address in _removing)
            {
                comp.Cyborgs.Remove(address);
            }

            if (_removing.Count > 0)
                UpdateUserInterface((uid, comp));
        }
    }

    private void OnPacketReceived(Entity<RoboticsConsoleComponent> ent, ref DeviceNetworkPacketEvent args)
    {
        var payload = args.Data;
        if (!payload.TryGetValue(DeviceNetworkConstants.Command, out string? command))
            return;
        if (command != DeviceNetworkConstants.CmdUpdatedState)
            return;

        if (!payload.TryGetValue(RoboticsConsoleConstants.NET_CYBORG_DATA, out CyborgControlData? data))
            return;

        var real = data.Value;
        real.Timeout = _timing.CurTime + ent.Comp.Timeout;
        ent.Comp.Cyborgs[args.SenderAddress] = real;

        UpdateUserInterface(ent);
    }

    private void OnOpened(Entity<RoboticsConsoleComponent> ent, ref BoundUIOpenedEvent args)
    {
        UpdateUserInterface(ent);
    }


    private void OnDisable(Entity<RoboticsConsoleComponent> ent, ref RoboticsConsoleDisableMessage args)
    {
        if (!ent.Comp.AllowBorgControl)
            return;

        if (_lock.IsLocked(ent.Owner))
            return;

        if (!ent.Comp.Cyborgs.TryGetValue(args.Address, out var data))
            return;

        var payload = new NetworkPayload()
        {
            [DeviceNetworkConstants.Command] = RoboticsConsoleConstants.NET_DISABLE_COMMAND
        };

        _deviceNetwork.QueuePacket(ent, args.Address, payload);
        _adminLogger.Add(LogType.Action, LogImpact.High, $"{ToPrettyString(args.Actor):user} disabled borg {data.Name} with address {args.Address}");
    }

    private void OnDestroy(Entity<RoboticsConsoleComponent> ent, ref RoboticsConsoleDestroyMessage args)
    {
        if (!ent.Comp.AllowBorgControl)
            return;

        if (_lock.IsLocked(ent.Owner))
            return;

        var now = _timing.CurTime;
        if (now < ent.Comp.NextDestroy)
            return;

        if (!ent.Comp.Cyborgs.Remove(args.Address, out var data))
            return;

        var payload = new NetworkPayload()
        {
            [DeviceNetworkConstants.Command] = RoboticsConsoleConstants.NET_DESTROY_COMMAND
        };

        _deviceNetwork.QueuePacket(ent, args.Address, payload);

        var message = Loc.GetString(ent.Comp.DestroyMessage, ("name", data.Name));
        _radio.SendRadioMessage(ent, message, ent.Comp.RadioChannel, ent);
        _adminLogger.Add(LogType.Action, LogImpact.Extreme, $"{ToPrettyString(args.Actor):user} destroyed borg {data.Name} with address {args.Address}");

        ent.Comp.NextDestroy = now + ent.Comp.DestroyCooldown;
        Dirty(ent, ent.Comp);
    }

    private void OnHack(Entity<RoboticsConsoleComponent> ent, ref RoboticsConsoleHackMessage args)
    {
        if (!ent.Comp.AllowBorgControl)
            return;

        if (!TryGetMalfAi(args.Actor, out var malfAi))
            return;

        if (!ent.Comp.Cyborgs.TryGetValue(args.Address, out var data))
            return;

        var payload = new NetworkPayload()
        {
            [DeviceNetworkConstants.Command] = RoboticsConsoleConstants.NET_HACK_COMMAND,
            [RoboticsConsoleConstants.NET_MALF_AI_CONTROLLER] = GetNetEntity(malfAi)
        };

        _deviceNetwork.QueuePacket(ent, args.Address, payload);
        _adminLogger.Add(LogType.Action, LogImpact.High, $"{ToPrettyString(args.Actor):user} (Malf AI {ToPrettyString(malfAi):entity}) hacked borg {data.Name} with address {args.Address}");
    }

    private bool TryGetMalfAi(EntityUid actor, out EntityUid malfAi)
    {
        if (HasComp<MalfAiMarkerComponent>(actor))
        {
            malfAi = actor;
            return true;
        }

        if (TryComp<StationAiEyeComponent>(actor, out _))
        {
            var coreQuery = AllEntityQuery<StationAiCoreComponent>();
            while (coreQuery.MoveNext(out var coreUid, out var coreComp))
            {
                if (coreComp.RemoteEntity == actor)
                {
                    if (_stationAi.TryGetHeld((coreUid, coreComp), out var held) &&
                        HasComp<MalfAiMarkerComponent>(held))
                    {
                        malfAi = held.Value;
                        return true;
                    }
                }
            }
        }

        if (TryComp<StationAiCoreComponent>(actor, out var coreComp2) &&
            _stationAi.TryGetHeld((actor, coreComp2), out var heldCore) &&
            HasComp<MalfAiMarkerComponent>(heldCore))
        {
            malfAi = heldCore.Value;
            return true;
        }

        if (_stationAi.TryGetCore(actor, out var core) &&
            _stationAi.TryGetHeld(core, out var heldBrain) &&
            HasComp<MalfAiMarkerComponent>(heldBrain))
        {
            malfAi = heldBrain.Value;
            return true;
        }

        if (TryComp<MindContainerComponent>(actor, out var mindContainer) &&
            mindContainer.Mind != null)
        {
            var mindQuery = AllEntityQuery<MalfAiMarkerComponent, MindContainerComponent>();
            while (mindQuery.MoveNext(out var mUid, out _, out var mMindContainer))
            {
                if (mMindContainer.Mind == mindContainer.Mind)
                {
                    malfAi = mUid;
                    return true;
                }
            }
        }

        malfAi = default;
        return false;
    }

    private void UpdateUserInterface(Entity<RoboticsConsoleComponent> ent)
    {
        var state = new RoboticsConsoleState(ent.Comp.Cyborgs, ent.Comp.AllowBorgControl);
        _ui.SetUiState(ent.Owner, RoboticsConsoleUiKey.Key, state);
    }
}
