// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Shared._Funkystation.MalfAI.VoiceModulator;
using Robust.Client.UserInterface;

namespace Content.Client._Funkystation.MalfAI.VoiceModulator;

/// <summary>
/// Client system that opens the voice modulator window when triggered.
/// </summary>
public sealed class MalfVoiceModulatorSystem : EntitySystem
{
    [Dependency] private readonly IUserInterfaceManager _uiManager = default!;

    private MalfVoiceModulatorWindow? _window;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<MalfVoiceModulatorOpenUiEvent>(OnOpenUi);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _window?.Dispose();
        _window = null;
    }

    private void OnOpenUi(MalfVoiceModulatorOpenUiEvent ev)
    {
        if (_window == null || _window.Disposed)
        {
            _window = new MalfVoiceModulatorWindow();
            _window.OnNameSubmitted += (name, verb, jobIcon) =>
            {
                RaiseNetworkEvent(new MalfVoiceModulatorSubmitNameEvent(name, verb, jobIcon));
                _window?.Close();
            };
        }

        _window.OpenCentered();
        _window.MoveToFront();
    }
}
