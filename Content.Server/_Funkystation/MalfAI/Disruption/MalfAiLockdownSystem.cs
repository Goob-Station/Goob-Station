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

namespace Content.Server._Funkystation.MalfAI.Disruption;

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

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<MalfAiLockdownActiveComponent>();
        while (query.MoveNext(out var uid, out var lockdown))
        {
            if (now < lockdown.EndTime)
                continue;

            if (!Deleted(lockdown.Grid))
            {
                _doorsBuffer.Clear();
                _lookup.GetChildEntities(lockdown.Grid, _doorsBuffer);

                foreach (var door in _doorsBuffer)
                {
                    if (door.Comp.BoltsDown)
                        _doorSystem.SetBoltsDown(door, false);
                }
            }

            RemComp<MalfAiLockdownActiveComponent>(uid);
        }
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

        _chat.DispatchStationAnnouncement(
            ent.Owner,
            Loc.GetString("malfai-lockdown-announce", ("duration", (int)args.Duration)),
            Loc.GetString("malfai-lockdown-sender"),
            colorOverride: Color.Red);

        _adminLog.Add(LogType.Action, LogImpact.High,
            $"Malf AI {ToPrettyString(ent.Owner)} initiated grid lockdown, bolted {boltedCount} doors");

        var lockdown = EnsureComp<MalfAiLockdownActiveComponent>(ent.Owner);
        lockdown.Grid = grid;
        lockdown.EndTime = _timing.CurTime + TimeSpan.FromSeconds(args.Duration);

        args.Handled = true;
    }
}
