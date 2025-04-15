// SPDX-FileCopyrightText: 2023 CommieFlowers <rasmus.cedergren@hotmail.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 rolfero <45628623+rolfero@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mervill <mervills.email@gmail.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.MachineLinking;
using Robust.Client.UserInterface;

namespace Content.Client.MachineLinking.UI;

public sealed class SignalTimerBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private SignalTimerWindow? _window;

    public SignalTimerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<SignalTimerWindow>();
        _window.OnStartTimer += StartTimer;
        _window.OnCurrentTextChanged += OnTextChanged;
        _window.OnCurrentDelayMinutesChanged += OnDelayChanged;
        _window.OnCurrentDelaySecondsChanged += OnDelayChanged;
    }

    public void StartTimer()
    {
        SendMessage(new SignalTimerStartMessage());
    }

    private void OnTextChanged(string newText)
    {
        SendMessage(new SignalTimerTextChangedMessage(newText));
    }

    private void OnDelayChanged(string newDelay)
    {
        if (_window == null)
            return;
        SendMessage(new SignalTimerDelayChangedMessage(_window.GetDelay()));
    }

    /// <summary>
    /// Update the UI state based on server-sent info
    /// </summary>
    /// <param name="state"></param>
    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_window == null || state is not SignalTimerBoundUserInterfaceState cast)
            return;

        _window.SetCurrentText(cast.CurrentText);
        _window.SetCurrentDelayMinutes(cast.CurrentDelayMinutes);
        _window.SetCurrentDelaySeconds(cast.CurrentDelaySeconds);
        _window.SetShowText(cast.ShowText);
        _window.SetTriggerTime(cast.TriggerTime);
        _window.SetTimerStarted(cast.TimerStarted);
        _window.SetHasAccess(cast.HasAccess);
    }
}