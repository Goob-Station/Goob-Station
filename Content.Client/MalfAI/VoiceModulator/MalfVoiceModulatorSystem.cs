// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Client.MalfAI.Theme;
using Content.Shared.MalfAI;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.GameObjects;

namespace Content.Client.MalfAI.VoiceModulator;

public sealed class MalfVoiceModulatorSystem : EntitySystem
{
    [Dependency] private readonly IResourceCache _res = default!;

    private MalfVoiceModulatorWindow? _window;
    private Font? _malfFont;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<MalfVoiceModulatorOpenUiEvent>(OnOpenUi);
        SubscribeNetworkEvent<MalfVoiceModulatorStateEvent>(OnStateUpdate);
    }

    private void OnOpenUi(MalfVoiceModulatorOpenUiEvent ev)
    {
        EnsureWindow();
        _window!.SetState(ev.Enabled, ev.VoiceName, ev.JobProtoId, ev.Accent);
        _window.OpenCentered();
        _window.MoveToFront();
    }

    private void OnStateUpdate(MalfVoiceModulatorStateEvent ev)
    {
        if (_window != null && _window.IsOpen)
        {
            _window.SetState(ev.Enabled, ev.VoiceName, ev.JobProtoId, ev.Accent);
        }
    }

    private void EnsureWindow()
    {
        if (_window != null)
            return;

        _malfFont ??= MalfUiTheme.GetFont(_res, 12);
        _window = new MalfVoiceModulatorWindow(_malfFont);
        _window.OnUpdate += (enabled, name, jobProtoId, accent) =>
        {
            RaiseNetworkEvent(new MalfVoiceModulatorUpdateEvent(enabled, name, jobProtoId, accent));
        };
    }
}
