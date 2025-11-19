// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿using Content.Client.UserInterface.Controls;

namespace Content.Client.Viewport
{
    /// <summary>
    ///     Client state that has a main viewport.
    /// </summary>
    /// <remarks>
    ///     Used for taking no-UI screenshots (including things like flash overlay).
    /// </remarks>
    public interface IMainViewportState
    {
        public MainViewport Viewport { get; }
    }
}
