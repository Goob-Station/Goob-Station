// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Dreykor <Dreykor12@gmail.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 ImHoks <142083149+ImHoks@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 ImHoks <imhokzzzz@gmail.com>
// SPDX-FileCopyrightText: 2025 KillanGenifer <killangenifer@gmail.com>
// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 funkystationbot <funky@funkystation.org>
// SPDX-FileCopyrightText: 2025 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration.Logs;
using Content.Server.DeviceNetwork.Systems;
using Content.Server.Radio.EntitySystems;
using Content.Shared.Lock;
using Content.Shared.Database;
using Content.Shared.DeviceNetwork;
using Content.Shared.Robotics;
using Content.Shared.Robotics.Components;
using Content.Shared.Robotics.Systems;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Store.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;
using Content.Shared.DeviceNetwork.Events;
using Content.Shared._Funkystation.MalfAI;
using Content.Shared.Alert;
using Content.Shared.Mind.Components;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.Utility;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Content.Shared.DeviceNetwork.Components;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Damage;
using Content.Shared.Mobs.Systems;
using Content.Shared._Funkystation.MalfAI.Components;
using Content.Server.Store.Systems;
using Content.Shared._Gabystation.MalfAi.Components;
using Content.Shared._Funkystation.CCVar;

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
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly StoreSystem _store = default!;

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
            subs.Event<RoboticsConsoleImposeLawMessage>(OnImposeLaw); // Funkystation -> Malf Ai
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

    // Funkystation -> Malf Ai
    private void OnImposeLaw(Entity<RoboticsConsoleComponent> ent, ref RoboticsConsoleImposeLawMessage args)
    {
        // Only Malf AI may impose Law 0.
        if (!TryComp<MalfunctioningAiComponent>(args.Actor, out var malfComp)
            || !HasComp<StationAiHeldComponent>(args.Actor))
            return;

        if (!ent.Comp.Cyborgs.TryGetValue(args.Address, out var data))
            return;

        // Do not allow imposing law on an already emagged borg (single-use). UI should grey out, but server enforces too.
        if (data.Emagged)
            return;

        // Charge CPU from the AI's store before sending the command.
        if (!TryComp<StoreComponent>(args.Actor, out var store))
            return;

        // Read CVAR cost and convert to FixedPoint2 for balance ops
        var imposeCost = FixedPoint2.New(_cfg.GetCVar(CCVarsMalfAi.MalfAiImposeLawCpuCost));

        if (!store.Balance.TryGetValue(malfComp.CurrencyId, out var balance)
            || balance < imposeCost)
            return; // insufficient CPU

        _store.TryAddCurrency(new() { [malfComp.CurrencyId] = -imposeCost }, args.Actor, store);

        // Link by device address across any device network (robust fallback)
        var linked = false;
        var queryAll = AllEntityQuery<DeviceNetworkComponent>();
        while (queryAll.MoveNext(out var targetUid, out var targetDev))
        {
            if (!string.Equals(targetDev.Address, args.Address, StringComparison.Ordinal))
                continue;

            var ctrl = EnsureComp<MalfAiControlledComponent>(targetUid);
            ctrl.Controller = args.Actor;
            if (string.IsNullOrWhiteSpace(ctrl.UniqueId))
                ctrl.UniqueId = $"BORG-{Guid.NewGuid().ToString("N").Substring(0, 8)}";
            Dirty(targetUid, ctrl);
            linked = true;
            break;
        }

        // If linked, push an immediate UI refresh if open on any AI entity that shares the same Mind as the actor.
        if (linked)
        {
            var state = BuildMalfBorgsState(args.Actor);
            // Refresh on the actor entity if open
            if (_ui.IsUiOpen((args.Actor, null), MalfAiBorgsUiKey.Key))
                _ui.SetUiState((args.Actor, null), MalfAiBorgsUiKey.Key, state);

            // Refresh on other AI entities with the same mind (e.g., AiHeld/Brain counterpart)
            if (TryComp<MindContainerComponent>(args.Actor, out var mindCont) && mindCont.Mind != null)
            {
                var targetMind = mindCont.Mind.Value;
                var mindQuery = AllEntityQuery<MindContainerComponent>();
                while (mindQuery.MoveNext(out var otherEnt, out var otherMindCont))
                {
                    if (otherEnt == args.Actor)
                        continue;
                    if (otherMindCont.Mind != targetMind)
                        continue;
                    if (_ui.IsUiOpen((otherEnt, null), MalfAiBorgsUiKey.Key))
                        _ui.SetUiState((otherEnt, null), MalfAiBorgsUiKey.Key, state);
                }
            }
        }

        var payload = new NetworkPayload()
        {
            [DeviceNetworkConstants.Command] = RoboticsConsoleConstants.NET_IMPOSE_LAW0_COMMAND
        };

        _deviceNetwork.QueuePacket(ent, args.Address, payload);
        _adminLogger.Add(LogType.Action, LogImpact.High, $"{ToPrettyString(args.Actor):user} imposed Law 0 on borg {data.Name} with address {args.Address} (CPU cost: {imposeCost})");
    }

    private void UpdateUserInterface(Entity<RoboticsConsoleComponent> ent)
    {
        var state = new RoboticsConsoleState(ent.Comp.Cyborgs);
        _ui.SetUiState(ent.Owner, RoboticsConsoleUiKey.Key, state);
    }

    // // Funkystation -> Malf Ai. Build the Malf AI Borgs UI state for the specified controller.
    private MalfAiBorgsUiState BuildMalfBorgsState(EntityUid controller)
    {
        var entries = new List<MalfAiBorgListEntry>();

        // Mind-aware matching to handle AiHeld vs StationAiBrain identity differences
        EntityUid? controllerMind = null;
        if (TryComp<MindContainerComponent>(controller, out var mindCont) && mindCont.Mind != null)
            controllerMind = mindCont.Mind.Value;

        var query = AllEntityQuery<MalfAiControlledComponent>();
        while (query.MoveNext(out var uid, out var ctrl))
        {
            var ok = false;
            if (ctrl.Controller == controller)
                ok = true;
            else if (controllerMind != null && ctrl.Controller != null)
            {
                if (TryComp<MindContainerComponent>(ctrl.Controller.Value, out var otherMindCont) && otherMindCont.Mind == controllerMind)
                    ok = true;
            }

            if (!ok)
                continue;

            var id = string.IsNullOrWhiteSpace(ctrl.UniqueId) ? ToPrettyString(uid) : ctrl.UniqueId!;
            var name = MetaData(uid).EntityName;
            SpriteSpecifier? sprite = null;
            if (TryComp<BorgTransponderComponent>(uid, out var trans))
                sprite = trans.Sprite;

            // Compute health as 1 - percent-to-death threshold if available.
            var health = 1.0f;
            if (TryComp<DamageableComponent>(uid, out var dmg))
            {
                if (_mobThreshold.TryGetDeadPercentage(uid, dmg.TotalDamage, out var pct))
                {
                    var frac = (float) FixedPoint2.Min(1.0f, pct!.Value).Float();
                    health = Math.Clamp(1.0f - frac, 0f, 1f);
                }
            }

            entries.Add(new MalfAiBorgListEntry(id, name, sprite, health));
        }
        return new MalfAiBorgsUiState(entries);
    }
}
