// Monolith - This file is licensed under AGPLv3
// Copyright (c) 2025 Monolith
// See AGPLv3.txt for details.

using Content.Server.DeviceLinking.Systems;
using Content.Server.Shuttles.Components;
using Content.Shared._NF.Shuttles.Events;
using Content.Shared.DeviceLinking;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.Shuttles.Components;

namespace Content.Server.Shuttles.Systems;

public sealed partial class ShuttleConsoleSystem
{
    [Dependency] private readonly DeviceLinkSystem _deviceLink = default!;

    private void InitializeNf()
    {
        SubscribeLocalEvent<ShuttleConsoleComponent, ComponentStartup>(OnConsoleStartup);

        Subs.BuiEvents<ShuttleConsoleComponent>(ShuttleConsoleUiKey.Key, subs =>
        {
            subs.Event<ShuttlePortButtonPressedMessage>(OnShuttlePortButtonPressed);
        });
    }

    private void OnShuttlePortButtonPressed(EntityUid uid, ShuttleConsoleComponent component, ShuttlePortButtonPressedMessage args)
    {
        _deviceLink.SendSignal(uid, args.SourcePort, true);
    }

    private void OnConsoleStartup(EntityUid uid, ShuttleConsoleComponent component, ComponentStartup args)
    {
        // The implementation seems to be missing, but it's referenced in ShuttleConsoleSystem.cs
        // We'll handle updating the state and ensuring device link components
        DockingInterfaceState? dockState = null;
        UpdateState(uid, ref dockState);

        // Also ensure device link components are added for our port buttons
        EnsureDeviceLinkComponents(uid, component);
    }

    private void EnsureDeviceLinkComponents(EntityUid uid, ShuttleConsoleComponent component)
    {
        if (!TryComp<DeviceLinkSourceComponent>(uid, out var sourceComp))
            return;

        _deviceLink.EnsureSourcePorts(uid, component.SourcePorts.ToArray());

        // Clear all signal states to prevent unwanted signals when establishing new connections
        foreach (var sourcePort in component.SourcePorts)
            _deviceLink.ClearSignal((uid, sourceComp), sourcePort);
    }
}
