// SPDX-FileCopyrightText: 2025 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
namespace Content.DiscordBot;

public sealed class Config
{
    public string Token { get; set; } = string.Empty;

    public string DatabaseString { get; set; } = string.Empty;

    public string GuildID { get; set; } = string.Empty;
}
