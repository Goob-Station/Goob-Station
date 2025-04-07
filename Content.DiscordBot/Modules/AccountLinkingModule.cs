// SPDX-FileCopyrightText: 2025 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Discord;
using Discord.Commands;

namespace Content.DiscordBot.Modules;

public sealed class AccountLinkingModule : ModuleBase<SocketCommandContext>
{
    [Command("create")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public Task CreateAsync()
    {
        var component = new ComponentBuilder()
            .WithButton("Link your SS14 account here!", "link-ss14-account")
            .Build();

        return ReplyAsync(string.Empty, components: component);
    }
}