// SPDX-FileCopyrightText: 2025 Dreykor <160512778+Dreykor@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Kyoth25f <kyoth25f@gmail.com>
// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 funkystationbot <funky@funkystation.org>
//
// SPDX-License-Identifier: MIT

using Content.Goobstation.Shared.IntrinsicVoiceModulator;
using Content.Shared.Speech;
using Content.Shared.StatusIcon;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.IntrinsicVoiceModulator;

public sealed class IntrinsicVoiceModulatorBoundUserInterface(EntityUid owner, Enum uiKey)
    : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private IntrinsicVoiceModulatorWindow? _window;

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<IntrinsicVoiceModulatorWindow>();

        _window.ReloadVerbs();
        _window.AddVerbs();

        _window.ReloadJobIcons();
        _window.AddJobIcons();

        _window.OnNameChanged += OnNameChanged;
        _window.OnJobIconChanged += OnJobIconChanged;
        _window.OnVerbChange += OnVerbChanged;
    }

    private void OnNameChanged(string newName)
    {
        SendMessage(new IntrinsicVoiceModulatorNameChangedMessage(newName));
    }

    public void OnJobIconChanged(ProtoId<JobIconPrototype> newJobIconId)
    {
        SendMessage(new IntrinsicVoiceModulatorJobIconChangedMessage(newJobIconId));
    }

    public void OnVerbChanged(ProtoId<SpeechVerbPrototype>? protoId)
    {
        SendMessage(new IntrinsicVoicemodulatorVerbChangedMessage(protoId));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_window is null
            || state is not IntrinsicVoiceModulatorBoundUserInterfaceState cast)
            return;

        _window.SetCurrentName(cast.CurrentName);
        _window.SetCurrentSpeechVerb(cast.CurrentVerb);
        _window.SetCurrentJobIcon(cast.JobIcon);
    }
}

