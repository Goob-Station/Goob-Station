// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using Content.Goobstation.Maths.FixedPoint;
using Content.Server.Administration.Logs;
using Content.Server.Construction.Components;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Power.Components;
using Content.Shared.Audio;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Database;
using Content.Shared.MalfAI;
using Content.Shared.MalfAI.Actions;
using Content.Shared.Popups;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Stunnable;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Localization;
using Robust.Shared.Map;
using Robust.Shared.Maths;

namespace Content.Server.MalfAI;

/// <summary>
/// Handles the Malf AI Overload Machine ability.
/// Detonates targeted machines, guaranteeing severe damage to anyone caught nearby.
/// </summary>
public sealed class MalfAiOverloadSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ExplosionSystem _explosion = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly Content.Server.Silicons.StationAi.StationAiSystem _stationAi = default!;

    private const float BlastRadius = 3.0f;
    private const float PointBlankRadius = 1.5f;

    private static readonly DamageSpecifier PointBlankDamage = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            { "Blunt", 40 },
            { "Heat", 30 },
            { "Piercing", 20 },
            { "Slash", 10 }
        }
    };

    private static readonly DamageSpecifier NearDamage = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            { "Blunt", 25 },
            { "Heat", 20 },
            { "Piercing", 10 },
            { "Slash", 5 }
        }
    };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationAiHeldComponent, MalfAiOverloadMachineActionEvent>(OnOverloadMachine);
    }

    private void OnOverloadMachine(Entity<StationAiHeldComponent> ai, ref MalfAiOverloadMachineActionEvent args)
    {
        var popupTarget = GetAiEyeForPopup(ai.Owner) ?? ai.Owner;

        // Only malf AIs can use this.
        if (!HasComp<MalfAiMarkerComponent>(ai))
        {
            _popup.PopupEntity(Loc.GetString("malfai-overload-not-malf"), popupTarget, ai);
            return;
        }

        // Get the target entity at the clicked coordinates
        var coords = args.Target;
        var mapCoords = coords.ToMap(EntityManager, _transform);

        if (mapCoords.MapId == MapId.Nullspace)
        {
            _popup.PopupEntity(Loc.GetString("malfai-overload-invalid-location"), popupTarget, ai);
            args.Handled = true;
            return;
        }

        // Find candidate machines within reach of clicked coordinates (pick closest)
        var entitiesAtLocation = _lookup.GetEntitiesInRange(mapCoords, 0.75f);
        EntityUid? targetMachine = null;
        var closestDist = float.MaxValue;

        foreach (var entity in entitiesAtLocation)
        {
            if (HasComp<MachineComponent>(entity) || HasComp<ApcPowerReceiverComponent>(entity))
            {
                var dist = (coords.Position - _transform.GetWorldPosition(entity)).LengthSquared();
                if (dist < closestDist)
                {
                    closestDist = dist;
                    targetMachine = entity;
                }
            }
        }

        if (targetMachine == null)
        {
            _popup.PopupEntity(Loc.GetString("malfai-overload-no-machine"), popupTarget, ai);
            args.Handled = true;
            return;
        }

        // Check if the machine is powered
        if (TryComp<ApcPowerReceiverComponent>(targetMachine.Value, out var powerReceiver) && !powerReceiver.Powered)
        {
            _popup.PopupEntity(Loc.GetString("malfai-overload-not-powered"), popupTarget, ai);
            args.Handled = true;
            return;
        }

        var machineUid = targetMachine.Value;
        var machinePos = _transform.GetMapCoordinates(machineUid);

        // Pre-blast audio effect
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Effects/sparks4.ogg"), machineUid);

        // Detonate the machine with a high-intensity explosion
        _explosion.QueueExplosion(
            epicenter: machinePos,
            typeId: ExplosionSystem.DefaultExplosionPrototypeId,
            totalIntensity: 120f,
            slope: 1.5f,
            maxTileIntensity: 20f,
            cause: ai.Owner,
            canCreateVacuum: false,
            maxTileBreak: 0);

        // Guarantee severe damage and knockdown to anyone caught near the detonating machine
        var nearbyEntities = _lookup.GetEntitiesInRange(machinePos, BlastRadius);
        foreach (var nearby in nearbyEntities)
        {
            if (nearby == machineUid || nearby == ai.Owner)
                continue;

            if (!HasComp<DamageableComponent>(nearby))
                continue;

            var dist = (machinePos.Position - _transform.GetWorldPosition(nearby)).Length();
            if (dist <= PointBlankRadius)
            {
                _damageable.TryChangeDamage(nearby, PointBlankDamage, ignoreResistances: false, origin: ai.Owner);
                _stun.TryKnockdown(nearby, TimeSpan.FromSeconds(4), refresh: true);
            }
            else if (dist <= BlastRadius)
            {
                _damageable.TryChangeDamage(nearby, NearDamage, ignoreResistances: false, origin: ai.Owner);
                _stun.TryKnockdown(nearby, TimeSpan.FromSeconds(2), refresh: true);
            }
        }

        // Log the detonation for admin records
        _adminLogger.Add(LogType.Explosion, LogImpact.High, $"Malf AI {ToPrettyString(ai.Owner)} overloaded and detonated machine {ToPrettyString(machineUid)} at {machinePos}");

        // Obliterate the overloaded machine
        QueueDel(machineUid);

        _popup.PopupEntity(Loc.GetString("malfai-overload-success"), popupTarget, ai);
        args.Handled = true;
    }

    /// <summary>
    /// Gets the AI eye entity for popup positioning, falls back to core if eye unavailable
    /// </summary>
    private EntityUid? GetAiEyeForPopup(EntityUid aiUid)
    {
        if (!_stationAi.TryGetCore(aiUid, out var core) || core.Comp?.RemoteEntity == null)
            return null;

        return core.Comp.RemoteEntity.Value;
    }
}
