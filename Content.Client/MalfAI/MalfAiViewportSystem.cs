// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Client.Eye;
using Content.Shared.MalfAI;
using Robust.Client.UserInterface;
using Robust.Shared.GameObjects;

namespace Content.Client.MalfAI;

/// <summary>
/// Receives server requests to open the Malf AI viewport window.
/// </summary>
public sealed class MalfAiViewportSystem : EntitySystem
{
    [Dependency] private readonly IUserInterfaceManager _ui = default!;
    [Dependency] private readonly EyeLerpingSystem _eyeLerpingSystem = default!;

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
