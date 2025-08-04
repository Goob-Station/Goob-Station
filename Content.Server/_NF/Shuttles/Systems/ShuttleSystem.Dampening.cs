// New Frontiers - This file is licensed under AGPLv3
// Copyright (c) 2024 New Frontiers Contributors
// See AGPLv3.txt for details.
using Content.Server.Shuttles.Components;
using Content.Shared._NF.Shuttles.Events;

namespace Content.Server.Shuttles.Systems;

public sealed partial class ShuttleSystem
{
    private const float SpaceFrictionStrength = 0.0075f;
    private const float DampenDampingStrength = 0.25f;
    private const float AnchorDampingStrength = 2.5f;

    private void InitializeNf()
    {
        SubscribeLocalEvent<ShuttleConsoleComponent, SetInertiaDampeningRequest>(OnSetInertiaDampening);
    }

    private bool SetInertiaDampening(Entity<ShuttleComponent> shuttle, InertiaDampeningMode mode)
    {
        var (uid, comp) = shuttle;

        if (mode == InertiaDampeningMode.Query)
        {
            _console.RefreshShuttleConsoles(uid);
            return false;
        }

        comp.BodyModifier = mode switch
        {
            InertiaDampeningMode.Off => SpaceFrictionStrength,
            InertiaDampeningMode.Dampen => DampenDampingStrength,
            InertiaDampeningMode.Anchor => AnchorDampingStrength,
            _ => DampenDampingStrength, // other values: default to some sane behaviour (assume normal dampening)
        };

        if (comp.DampingModifier != 0)
            comp.DampingModifier = comp.BodyModifier;

        _console.RefreshShuttleConsoles(uid);
        return true;
    }

    private void OnSetInertiaDampening(EntityUid uid, ShuttleConsoleComponent component, SetInertiaDampeningRequest args)
    {
        var targetShuttle = Transform(uid).GridUid;

        // Stupid cargo shuttle doesn't let you change dampening remotely.
        if (TryComp<DroneConsoleComponent>(uid, out var cargoConsole))
            targetShuttle = cargoConsole.Entity;

        if (!TryComp(targetShuttle, out ShuttleComponent? shuttleComponent))
            return;

        if (SetInertiaDampening((targetShuttle.Value, shuttleComponent), args.Mode)
            && args.Mode != InertiaDampeningMode.Query)
            component.DampeningMode = args.Mode;
    }

    public InertiaDampeningMode NfGetInertiaDampeningMode(EntityUid entity)
    {
        if (!EntityManager.TryGetComponent<TransformComponent>(entity, out var xform))
            return InertiaDampeningMode.Dampen;

        if (!EntityManager.TryGetComponent(xform.GridUid, out ShuttleComponent? shuttle))
            return InertiaDampeningMode.Dampen;

        if (shuttle.BodyModifier >= AnchorDampingStrength)
            return InertiaDampeningMode.Anchor;

        if (shuttle.BodyModifier <= SpaceFrictionStrength)
            return InertiaDampeningMode.Off;

        return InertiaDampeningMode.Dampen;
    }

    public void NfSetPowered(EntityUid uid, ShuttleConsoleComponent component, bool powered)
    {
        var targetShuttle = Transform(uid).GridUid;
        if (!TryComp(targetShuttle, out ShuttleComponent? shuttleComponent))
            return;

        // Update dampening physics without adjusting requested mode.
        if (!powered)
            SetInertiaDampening((targetShuttle.Value, shuttleComponent), InertiaDampeningMode.Anchor);
        else
        {
            // Update our dampening mode if we need to, and if we aren't a station.
            var currentDampening = NfGetInertiaDampeningMode(uid);
            if (currentDampening != component.DampeningMode
                && currentDampening != InertiaDampeningMode.Station
                && component.DampeningMode != InertiaDampeningMode.Station)
                SetInertiaDampening((targetShuttle.Value, shuttleComponent), component.DampeningMode);
        }
    }
}
