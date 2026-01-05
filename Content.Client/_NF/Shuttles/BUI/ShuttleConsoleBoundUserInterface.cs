// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

// New Frontiers - This file is licensed under AGPLv3
// Copyright (c) 2024 New Frontiers Contributors
// See AGPLv3.txt for details.
using Content.Client.Shuttles.UI;
using Content.Shared._NF.Shuttles.Events;

namespace Content.Client.Shuttles.BUI;

public sealed partial class ShuttleConsoleBoundUserInterface
{
    private void NfOpen()
    {
        _window ??= new ShuttleConsoleWindow();
        _window.OnInertiaDampeningModeChanged += OnInertiaDampeningModeChanged;
        _window.OnNetworkPortButtonPressed += OnNetworkPortButtonPressed;
    }
    private void OnInertiaDampeningModeChanged(NetEntity? entityUid, InertiaDampeningMode mode)
    {
        SendMessage(new SetInertiaDampeningRequest
        {
            ShuttleEntityUid = entityUid,
            Mode = mode,
        });
    }

    private void OnNetworkPortButtonPressed(string sourcePort)
    {
        SendMessage(new ShuttlePortButtonPressedMessage
        {
            SourcePort = sourcePort,
        });
    }
}
