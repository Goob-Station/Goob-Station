// New Frontiers - This file is licensed under AGPLv3
// Copyright (c) 2024 New Frontiers Contributors
// See AGPLv3.txt for details.
using Content.Server.Shuttles.Components;
using Content.Shared._NF.Shuttles.Events;

namespace Content.Server.Shuttles.Systems;

public sealed partial class ShuttleSystem
{
    private void InitializeNf()
    {
        SubscribeLocalEvent<ShuttleConsoleComponent, SetInertiaDampeningRequest>(OnSetInertiaDampening);
    }

    private bool SetInertiaDampening(Entity<ShuttleComponent> shuttle, InertiaDampeningMode mode)
    {
        var (uid, comp) = shuttle;

        if (mode == InertiaDampeningMode.None)
        {
            _console.RefreshShuttleConsoles(uid);
            return false;
        }

        comp.BodyModifier = comp.DampingModifiers[mode];

        if (comp.DampingModifier != 0)
            comp.DampingModifier = comp.BodyModifier;

        _console.RefreshShuttleConsoles(uid);
        return true;
    }

    private void OnSetInertiaDampening(EntityUid uid, ShuttleConsoleComponent component, SetInertiaDampeningRequest args)
    {
        var targetShuttle = Transform(uid).GridUid;

        // Stupid cargo shuttle doesn't let you change dampening remotely.
        if (TryComp(uid, out DroneConsoleComponent? cargoConsole))
            targetShuttle = cargoConsole.Entity;

        if (!TryComp(targetShuttle, out ShuttleComponent? shuttleComponent))
            return;

        if (SetInertiaDampening((targetShuttle.Value, shuttleComponent), args.Mode)
            && args.Mode != InertiaDampeningMode.None)
            component.DampeningMode = args.Mode;
    }

    public InertiaDampeningMode NfGetInertiaDampeningMode(EntityUid entity)
    {
        var xform = Transform(entity);

        if (!TryComp(xform.GridUid, out ShuttleComponent? shuttle))
            return InertiaDampeningMode.Dampen;

        if (shuttle.BodyModifier >= shuttle.DampingModifiers[InertiaDampeningMode.Anchor])
            return InertiaDampeningMode.Anchor;

        if (shuttle.BodyModifier <= shuttle.DampingModifiers[InertiaDampeningMode.Cruise])
            return InertiaDampeningMode.Cruise;

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
            // Update our dampening mode if we need to.
            var currentDampening = NfGetInertiaDampeningMode(uid);
            if (currentDampening != component.DampeningMode)
                SetInertiaDampening((targetShuttle.Value, shuttleComponent), component.DampeningMode);
        }
    }
}
