// SPDX-FileCopyrightText: 2023 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Client.UserInterface.Systems.Chat;
using Content.Shared.Chat;
using Robust.Client.Replays.Commands;
using Robust.Client.Replays.UI;
using Robust.Client.UserInterface;
using Robust.Shared.Console;

namespace Content.Client.Replay;

public sealed class ReplayToggleScreenshotModeCommand : BaseReplayCommand
{
    [Dependency] private readonly IUserInterfaceManager _userInterfaceManager = default!;
    [Dependency] private readonly ContentReplayPlaybackManager _replayManager = default!;

    public override string Command => "replay_toggle_screenshot_mode";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var screen = _userInterfaceManager.ActiveScreen;
        if (screen == null)
            return;

        _replayManager.IsScreenshotMode = !_replayManager.IsScreenshotMode;

        var showReplayWidget = _replayManager.IsScreenshotMode;
        screen.ShowWidget<ReplayControlWidget>(showReplayWidget);

        foreach (var chatBox in _userInterfaceManager.GetUIController<ChatUIController>().Chats)
        {
            chatBox.ChatInput.Visible = !showReplayWidget;
            if (!showReplayWidget)
                chatBox.ChatInput.ChannelSelector.Select(ChatSelectChannel.Local);
        }
    }
}