// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Explosion.EntitySystems;
using Content.Server.Power.Components;
using Content.Server.Construction.Components;
using Content.Shared._Funkystation.MalfAI.Actions;
using Content.Shared.Popups;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.Map;
using Content.Server.Silicons.StationAi;
using Content.Shared._Gabystation.MalfAi.Components;

namespace Content.Server._Funkystation.MalfAI;

/// <summary>
/// Handles the Malf AI Overload Machine ability.
/// Explodes targeted machines when activated.
/// </summary>
public sealed class MalfAiOverloadSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ExplosionSystem _explosion = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly StationAiSystem _stationAi = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationAiHeldComponent, MalfAiOverloadMachineActionEvent>(OnOverloadMachine);
    }

    private void OnOverloadMachine(Entity<StationAiHeldComponent> ai, ref MalfAiOverloadMachineActionEvent args)
    {
        var popupTarget = GetAiEyeForPopup(ai.Owner) ?? ai.Owner;

        // Only malf AIs can use this.
        if (!HasComp<MalfunctioningAiComponent>(ai))
        {
            _popup.PopupEntity(Loc.GetString("malfai-overload-not-malf"), popupTarget, ai);
            return;
        }

        // Get the target entity at the clicked coordinates
        var coords = args.Target;
        var mapCoords = _transform.ToMapCoordinates(coords);

        if (mapCoords.MapId == MapId.Nullspace)
        {
            _popup.PopupEntity(Loc.GetString("malfai-overload-invalid-location"), popupTarget, ai);
            return;
        }

        // Find entities at the target location
        var entitiesAtLocation = _lookup.GetEntitiesInRange(mapCoords, 0.5f);
        EntityUid? targetMachine = null;

        foreach (var entity in entitiesAtLocation)
        {
            // Match Override's targeting: operate only on actual machines
            if (HasComp<MachineComponent>(entity))
            {
                targetMachine = entity;
                break;
            }
        }

        if (targetMachine == null)
        {
            _popup.PopupEntity(Loc.GetString("malfai-overload-no-machine"), popupTarget, ai);
            return;
        }

        // Check if the machine is powered
        if (TryComp<ApcPowerReceiverComponent>(targetMachine.Value, out var powerReceiver) && !powerReceiver.Powered)
        {
            _popup.PopupEntity(Loc.GetString("malfai-overload-not-powered"), popupTarget, ai);
            return;
        }

        // Create a forced explosion even if the machine is not inherently explosive.
        var desiredRadius = 2.0f;
        var slope = 1.0f;
        var maxIntensity = 4.0f;
        var total = _explosion.RadiusToIntensity(desiredRadius, slope, maxIntensity);
        _explosion.QueueExplosion(mapCoords, ExplosionSystem.DefaultExplosionPrototypeId, total, slope, maxIntensity, targetMachine.Value);
        // Delete the machine afterwards to match the previous behavior of delete: true
        QueueDel(targetMachine.Value);

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
