// SPDX-FileCopyrightText: 2023 CommieFlowers <rasmus.cedergren@hotmail.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 rolfero <45628623+rolfero@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mervill <mervills.email@gmail.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.DeviceNetwork;
using Robust.Client.UserInterface;

namespace Content.Goobstation.Client.DeviceNetwork.UI;

public sealed class DeviceCustomFrequencyBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private DeviceCustomFrequencyWindow? _window;

    public DeviceCustomFrequencyBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<DeviceCustomFrequencyWindow>();
        _window.OnReceiveFrequencyChanged += OnReceiveChanged;
        _window.OnTransmitFrequencyChanged += OnTransmitChanged;
        _window.OnResetToDefault += OnReset;

        if (!EntMan.TryGetComponent(Owner, out DeviceCustomFrequencyComponent? deviceCustom))
            return;

        _window.MinReceiveFrequency = deviceCustom.MinReceiveFrequency;
        _window.MaxReceiveFrequency = deviceCustom.MaxReceiveFrequency;
        _window.MinTransmitFrequency = deviceCustom.MinTransmitFrequency;
        _window.MaxTransmitFrequency = deviceCustom.MaxTransmitFrequency;

        if (deviceCustom.ReceiveChange)
            _window.EnsureReceiveSpin(0);

        if (deviceCustom.TransmitChange)
            _window.EnsureTransmitSpin(0);
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (_window == null || state is not DeviceCustomFrequencyUserInterfaceState cast)
            return;

        if (cast.TransmitFrequency != null)
            _window.EnsureTransmitSpin(cast.TransmitFrequency.Value);

        if (cast.ReceiveFrequency != null)
            _window.EnsureReceiveSpin(cast.ReceiveFrequency.Value);
    }

    private void OnReset()
    {
        SendMessage(new DeviceCustomResetFrequencyMessage());
    }

    private void OnReceiveChanged(uint value)
    {
        SendMessage(new DeviceCustomReceiveFrequencyChangeMessage(value));
    }

    private void OnTransmitChanged(uint value)
    {
        SendMessage(new DeviceCustomTransmitFrequencyChangeMessage(value));
    }
}
