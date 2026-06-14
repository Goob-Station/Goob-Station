// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Server.Administration.Logs;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Power.Components;
using Content.Shared.Database;
using Content.Shared._Funkystation.MalfAI;
using Content.Shared._Funkystation.MalfAI.Actions;
using Content.Shared.Popups;

namespace Content.Server._Funkystation.MalfAI.Disruption;

/// <summary>
/// Overloads and explodes a targeted machine on the Malf AI's grid.
/// </summary>
public sealed class MalfAiOverloadSystem : EntitySystem
{
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly ExplosionSystem _explosion = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _xforms = default!;

    private readonly HashSet<Entity<TransformComponent>> _entityBuffer = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MalfAiMarkerComponent, MalfAiOverloadMachineActionEvent>(OnOverload);
    }

    private void OnOverload(Entity<MalfAiMarkerComponent> ent, ref MalfAiOverloadMachineActionEvent args)
    {
        if (args.Handled)
            return;

        // Find the closest APC-powered device in a very small radius around the clicked point.
        _entityBuffer.Clear();
        _lookup.GetEntitiesInRange(args.Target, 0.35f, _entityBuffer);

        var targetPos = _xforms.ToMapCoordinates(args.Target).Position;
        EntityUid? machine = null;
        var load = 0f;
        var bestDist = float.MaxValue;

        foreach (var nearby in _entityBuffer)
        {
            if (nearby.Owner == ent.Owner || !TryComp<ApcPowerReceiverComponent>(nearby.Owner, out var receiver))
                continue;

            var dist = (_xforms.GetWorldPosition(nearby.Comp) - targetPos).LengthSquared();
            if (dist < bestDist)
            {
                bestDist = dist;
                machine = nearby.Owner;
                load = receiver.Load;
            }
        }

        if (machine == null)
        {
            _popup.PopupCursor(Loc.GetString("malfai-overload-no-target"), ent.Owner, PopupType.Medium);
            return;
        }

        // Explosion scales with the device's power draw: a light barely pops,
        // a hungry machine blows hard.
        var intensity = Math.Clamp(load * 5f, 40f, 3000f);
        var tileIntensity = Math.Clamp(intensity / 2f, 20f, 600f);
        _explosion.QueueExplosion(machine.Value, "Default", totalIntensity: intensity, slope: 2f, maxTileIntensity: tileIntensity, maxTileBreak: 2);

        _popup.PopupCursor(Loc.GetString("malfai-overload-success"), ent.Owner, PopupType.Medium);

        _adminLog.Add(LogType.Action, LogImpact.High,
            $"Malf AI {ToPrettyString(ent.Owner)} overloaded machine {ToPrettyString(machine.Value)} (load {load}W, intensity {intensity})");

        args.Handled = true;
    }
}
