// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Client.UserInterface.CustomControls;

namespace Content.Client.Viewport
{
    public static class ViewportExt
    {
        public static int GetRenderScale(this IViewportControl viewport)
        {
            if (viewport is ScalingViewport svp)
                return svp.CurrentRenderScale;

            return 1;
        }
    }
}