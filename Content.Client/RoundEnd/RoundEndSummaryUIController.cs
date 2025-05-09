// SPDX-FileCopyrightText: 2024 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 SlamBamActionman <slambamactionman@gmail.com>
// SPDX-FileCopyrightText: 2024 wafehling <wafehling@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.GameTicking.Managers;
using Content.Shared.GameTicking;
using Content.Shared.Input;
using JetBrains.Annotations;
using Robust.Client.Input;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Input.Binding;
using Robust.Shared.Player;

namespace Content.Client.RoundEnd;

[UsedImplicitly]
public sealed class RoundEndSummaryUIController : UIController,
    IOnSystemLoaded<ClientGameTicker>
{
    [Dependency] private readonly IInputManager _input = default!;

    private RoundEndSummaryWindow? _window;

    private void ToggleScoreboardWindow(ICommonSession? session = null)
    {
        if (_window == null)
            return;

        if (_window.IsOpen)
        {
            _window.Close();
        }
        else
        {
            _window.OpenCenteredRight();
            _window.MoveToFront();
        }
    }

    public void OpenRoundEndSummaryWindow(RoundEndMessageEvent message)
    {
        // Don't open duplicate windows (mainly for replays).
        if (_window?.RoundId == message.RoundId)
            return;

        _window = new RoundEndSummaryWindow(message.GamemodeTitle, message.RoundEndText,
            message.RoundDuration, message.RoundId, message.AllPlayersEndInfo, EntityManager);
    }

    public void OnSystemLoaded(ClientGameTicker system)
    {
        _input.SetInputCommand(ContentKeyFunctions.ToggleRoundEndSummaryWindow,
            InputCmdHandler.FromDelegate(ToggleScoreboardWindow));
    }
}