// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

// New Frontiers - This file is licensed under AGPLv3
// Copyright (c) 2024 New Frontiers Contributors
// See AGPLv3.txt for details.
using Content.Shared._NF.Shuttles.Events;

namespace Content.Client.Shuttles.UI;

public sealed partial class ShuttleConsoleWindow
{
    public event Action<NetEntity?, InertiaDampeningMode>? OnInertiaDampeningModeChanged;

    public event Action<string>? OnNetworkPortButtonPressed;

    private void NfInitialize()
    {
        NavContainer.OnInertiaDampeningModeChanged += (entity, mode) =>
        {
            OnInertiaDampeningModeChanged?.Invoke(entity, mode);
        };

        NavContainer.OnNetworkPortButtonPressed += (sourcePort) =>
        {
            OnNetworkPortButtonPressed?.Invoke(sourcePort);
        };
    }
}
