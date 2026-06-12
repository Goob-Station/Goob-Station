// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Server.Administration.Logs;
using Content.Server.Chat.Systems;
using Content.Shared.Database;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Shared._Funkystation.MalfAI;
using Content.Shared._Funkystation.MalfAI.Actions;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;

namespace Content.Server._Funkystation.MalfAI;

/// <summary>
/// Handles the Malf AI lockdown ability - bolts and electrifies all doors on the grid.
/// </summary>
public sealed class MalfAiLockdownSystem : EntitySystem
{
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedDoorSystem _doorSystem = default!;

    private readonly HashSet<Entity<DoorBoltComponent>> _doorsBuffer = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MalfAiMarkerComponent, MalfAiLockdownGridActionEvent>(OnLockdown);
    }

    private void OnLockdown(Entity<MalfAiMarkerComponent> ent, ref MalfAiLockdownGridActionEvent args)
    {
        if (args.Handled)
            return;

        var xform = Transform(ent.Owner);
        if (xform.GridUid is not { } grid)
            return;

        _doorsBuffer.Clear();
        _lookup.GetChildEntities(grid, _doorsBuffer);

        var boltedCount = 0;
        foreach (var door in _doorsBuffer)
        {
            _doorSystem.SetBoltsDown(door, true);
            boltedCount++;
        }

        // Announce the lockdown
        _chat.DispatchStationAnnouncement(
            ent.Owner,
            Loc.GetString("malfai-lockdown-announce", ("duration", (int)args.Duration)),
            Loc.GetString("malfai-lockdown-sender"),
            colorOverride: Color.Red);

        _adminLog.Add(LogType.Action, LogImpact.High,
            $"Malf AI {ToPrettyString(ent.Owner)} initiated grid lockdown, bolted {boltedCount} doors");

        // Schedule door unbolt
        Timer.Spawn(TimeSpan.FromSeconds(args.Duration), () =>
        {
            if (Deleted(grid))
                return;

            _doorsBuffer.Clear();
            _lookup.GetChildEntities(grid, _doorsBuffer);

            foreach (var door in _doorsBuffer)
            {
                if (door.Comp.BoltsDown)
                    _doorSystem.SetBoltsDown(door, false);
            }
        });

        args.Handled = true;
    }
}
