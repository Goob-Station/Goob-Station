// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Client.MalfAI.Theme;
using Content.Shared.MalfAI;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface.Controls;
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
    }

    private void OnOpenUi(MalfVoiceModulatorOpenUiEvent ev)
    {
        _malfFont ??= MalfUiTheme.GetFont(_res, 12);

        _window ??= new MalfVoiceModulatorWindow(_malfFont);
        _window.OnConfirm += name =>
        {
            RaiseNetworkEvent(new MalfVoiceModulatorSubmitNameEvent(name));
            _window?.Close();
        };

        _window.OpenCentered();
        _window.MoveToFront();
    }
}
