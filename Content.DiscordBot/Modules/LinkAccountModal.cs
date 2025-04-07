// SPDX-FileCopyrightText: 2025 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Discord.Interactions;

namespace Content.DiscordBot.Modules;

public class LinkAccountModal : IModal
{
    public string Title => "Link SS14 account";

    [InputLabel("SS14 Linking Code (top left in the lobby)")]
    [RequiredInput]
    [ModalTextInput("account_code")]
    public string Code { get; set; } = string.Empty;
}
