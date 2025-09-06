// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

// New Frontiers - This file is licensed under AGPLv3
// Copyright (c) 2024 New Frontiers Contributors
// See AGPLv3.txt for details.
using Content.Shared._NF.Shuttles.Events;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.Shuttles.UI;

public sealed partial class NavScreen
{
    private readonly ButtonGroup _buttonGroup = new();
    public event Action<NetEntity?, InertiaDampeningMode>? OnInertiaDampeningModeChanged;

    public event Action<string>? OnNetworkPortButtonPressed;

    private void NfInitialize()
    {
        // Frontier - IFF search
        IffSearchCriteria.OnTextChanged += args => OnIffSearchChanged(args.Text);

        // Frontier - Maximum IFF Distance
        MaximumIFFDistanceValue.GetChild(0).GetChild(1).Margin = new Thickness(8, 0, 0, 0);
        MaximumIFFDistanceValue.OnValueChanged += args => OnRangeFilterChanged(args);

        DampenerOff.OnPressed += _ => SetDampenerMode(InertiaDampeningMode.Cruise);
        DampenerOn.OnPressed += _ => SetDampenerMode(InertiaDampeningMode.Dampen);
        AnchorOn.OnPressed += _ => SetDampenerMode(InertiaDampeningMode.Anchor);

        DampenerOff.Group = _buttonGroup;
        DampenerOn.Group = _buttonGroup;
        AnchorOn.Group = _buttonGroup;

        // Network Port Buttons
        DeviceButton1.OnPressed += _ => OnPortButtonPressed("SignalShuttleConsole1");
        DeviceButton2.OnPressed += _ => OnPortButtonPressed("SignalShuttleConsole2");
        DeviceButton3.OnPressed += _ => OnPortButtonPressed("SignalShuttleConsole3");
        DeviceButton4.OnPressed += _ => OnPortButtonPressed("SignalShuttleConsole4");

        // Send off a request to get the current dampening mode.
        _entManager.TryGetNetEntity(_shuttleEntity, out var shuttle);
        OnInertiaDampeningModeChanged?.Invoke(shuttle, InertiaDampeningMode.None);
    }

    private void OnPortButtonPressed(string sourcePort)
    {
        OnNetworkPortButtonPressed?.Invoke(sourcePort);
    }

    private void SetDampenerMode(InertiaDampeningMode mode)
    {
        NavRadar.DampeningMode = mode;
        _entManager.TryGetNetEntity(_shuttleEntity, out var shuttle);
        OnInertiaDampeningModeChanged?.Invoke(shuttle, mode);
    }

    private void NfUpdateState()
    {
        DampenerOff.Pressed = NavRadar.DampeningMode == InertiaDampeningMode.Cruise;
        DampenerOn.Pressed = NavRadar.DampeningMode == InertiaDampeningMode.Dampen;
        AnchorOn.Pressed = NavRadar.DampeningMode == InertiaDampeningMode.Anchor;

        // Disable the Park button (AnchorOn) while in FTL, but keep other dampener buttons enabled
        if (NavRadar.InFtl)
        {
            AnchorOn.Disabled = true;
            // If the AnchorOn button is pressed while it gets disabled, we need to switch to another mode
            if (!AnchorOn.Pressed)
                return;

            DampenerOn.Pressed = true;
            SetDampenerMode(InertiaDampeningMode.Dampen);
        }
        else
            AnchorOn.Disabled = false;
    }

    // Frontier - Maximum IFF Distance
    private void OnRangeFilterChanged(int value)
    {
        NavRadar.MaximumIFFDistance = value;
    }
}
