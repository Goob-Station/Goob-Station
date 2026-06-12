// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
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

    public override void Shutdown()
    {
        base.Shutdown();
        _window?.Close();
        _window = null;
    }

    private void OnOpenViewport(MalfAiViewportOpenEvent ev)
    {
        // Recreate the window each time: it configures its eye in the constructor and opens itself.
        _window?.Close();
        _window = new MalfAiViewportWindow(ev.MapId, ev.WorldPos, ev.SizePixels, ev.Title, ev.Rotation, ev.ZoomLevel, ev.AnchorEntity);
        _window.OnClose += () =>
        {
            _window = null;
            // Keep the server-side open/closed state in sync when the player closes via the X button.
            RaiseNetworkEvent(new MalfAiViewportWindowClosedEvent());
        };
    }

    private void OnCloseViewport(MalfAiViewportCloseEvent ev)
    {
        _window?.Close();
        _window = null;
    }
}
