// SPDX-FileCopyrightText: 2025 Dreykor <160512778+Dreykor@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Kyoth25f <kyoth25f@gmail.com>
// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 funkystationbot <funky@funkystation.org>
//
// SPDX-License-Identifier: MIT

using Content.Shared._Funkystation.MalfAI;

namespace Content.Client._Funkystation.MalfAI;

/// <summary>
/// Receives server requests to open the Malf AI viewport window.
/// </summary>
public sealed class MalfAiViewportSystem : EntitySystem
{
    private MalfAiViewportWindow? _window;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<MalfAiViewportOpenEvent>(OnOpenViewport);
        SubscribeNetworkEvent<MalfAiViewportCloseEvent>(OnCloseViewport);
    }

    private void OnOpenViewport(MalfAiViewportOpenEvent ev)
    {
        _window?.Close();
        _window = new MalfAiViewportWindow(ev.MapId, ev.WorldPosition, ev.SizePixels, ev.Title, ev.Rotation, ev.ZoomLevel, ev.AnchorEntity);
        _window.OnClose += () =>
        {
            _window = null;
        };
    }

    private void OnCloseViewport(MalfAiViewportCloseEvent ev)
    {
        _window?.Close();
        _window = null;
    }
}
