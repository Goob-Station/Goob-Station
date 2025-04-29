// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 DrSmugleaf <drsmugleaf@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ichaie <167008606+Ichaie@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 JORJ949 <159719201+JORJ949@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 MortalBaguette <169563638+MortalBaguette@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Panela <107573283+AgentePanela@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Poips <Hanakohashbrown@gmail.com>
// SPDX-FileCopyrightText: 2025 PuroSlavKing <103608145+PuroSlavKing@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 Whisper <121047731+QuietlyWhisper@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 blobadoodle <me@bloba.dev>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 github-actions[bot] <41898282+github-actions[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 kamkoi <poiiiple1@gmail.com>
// SPDX-FileCopyrightText: 2025 shibe <95730644+shibechef@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 tetra <169831122+Foralemes@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Immutable;
using System.Reflection;
using Content.DiscordBot.Modules;
using Content.Server.Database;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace Content.DiscordBot;

public sealed class CommandHandler(DiscordSocketClient client, CommandService commands, InteractionService interaction, PostgresServerDbContext db, ulong Guild)
{
    private ImmutableDictionary<ulong, RMCPatronTier>? _patronTiers;
    private ImmutableArray<RMCPatronTier> _tierPriority;
    private Task? _refreshPatronsTask;

    public int Running = 1;

    public async Task InstallCommandsAsync()
    {
        var patronTiers = await db.RMCPatronTiers.ToListAsync();
        _tierPriority = [..patronTiers.OrderBy(t => t.Priority)];
        _patronTiers = patronTiers.ToImmutableDictionary(t => t.DiscordRole, t => t);

        client.MessageReceived += HandleCommandAsync;
        client.ButtonExecuted += HandleButtonAsync;
        client.ModalSubmitted += HandleModalAsync;
        client.GuildMemberUpdated += HandleGuildMemberUpdated;

        await commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);

        interaction.AddModalInfo<LinkAccountModal>();

        _refreshPatronsTask = Task.Run(async () => await RefreshPatrons());
    }

    private async Task HandleGuildMemberUpdated(Cacheable<SocketGuildUser, ulong> old, SocketGuildUser user)
    {
        if (_patronTiers == null)
            return;

        var rolesChanged = !old.HasValue || old.Value.Roles.Count != user.Roles.Count || !old.Value.Roles.SequenceEqual(user.Roles);
        if (!rolesChanged)
            return;

        var wasPatron = old.HasValue || old.Value.Roles.Any(r => _patronTiers.ContainsKey(r.Id));
        var isPatron = user.Roles.Any(r => _patronTiers.ContainsKey(r.Id));
        if (wasPatron && !isPatron)
        {
            var linked = await db.RMCLinkedAccounts
                .Include(l => l.Player)
                .ThenInclude(p => p.Patron)
                .ThenInclude(p => p!.Tier)
                .Where(l => l.Player.Patron != null)
                .FirstOrDefaultAsync(p => p.DiscordId == user.Id);

            if (linked?.Player.Patron is { } patron)
            {
                db.RMCPatrons.Remove(patron);
                await db.SaveChangesAsync();
                await Logger.Info($"Removed patron {user.Username}:{linked.DiscordId}:{linked.Player.LastSeenUserName} with tier {patron.Tier.Name}");
            }

            return;
        }

        if (isPatron)
        {
            foreach (var tier in _tierPriority)
            {
                if (user.Roles.Any(r => r.Id == tier.DiscordRole))
                {
                    var linked = await db.RMCLinkedAccounts
                        .Include(l => l.Player)
                        .ThenInclude(p => p.Patron)
                        .FirstOrDefaultAsync(p => p.DiscordId == user.Id);

                    if (linked?.Player is not { } player)
                        return;

                    player.Patron ??= db.RMCPatrons.Add(new RMCPatron { PlayerId = player.UserId }).Entity;
                    player.Patron.TierId = tier.Id;
                    await db.SaveChangesAsync();
                    await Logger.Info($"Updated patron {user.Username}:{linked.DiscordId}:{linked.Player.LastSeenUserName} with tier {tier.Name}");
                    break;
                }
            }
        }
    }

    private async Task HandleCommandAsync(SocketMessage messageParam)
    {
        // Don't process the command if it was a system message
        var message = messageParam as SocketUserMessage;
        if (message == null)
            return;

        // Create a number to track where the prefix ends and the command begins
        var argPos = 0;

        // Determine if the message is a command based on the prefix and make sure no bots trigger commands
        if (!(message.HasCharPrefix('!', ref argPos) ||
            message.HasMentionPrefix(client.CurrentUser, ref argPos)) ||
            message.Author.IsBot)
            return;

        // Create a WebSocket-based command context based on the message
        var context = new SocketCommandContext(client, message);

        // Execute the command with the command context we just
        // created, along with the service provider for precondition checks.
        await commands.ExecuteAsync(context, argPos, null);
    }

    private async Task HandleButtonAsync(SocketMessageComponent component)
    {
        switch (component.Data.CustomId)
        {
            case "link-ss14-account":
                await component.RespondWithModalAsync<LinkAccountModal>("link-ss14-account");
                break;
        }
    }

    private async Task HandleModalAsync(SocketModal modal)
    {
        switch (modal.Data.CustomId)
        {
            case "link-ss14-account":
                if (modal.GuildId is not { } guildId)
                    break;

                var codeStr = modal.Data.Components.First(c => c.CustomId == "account_code").Value.Trim();
                if (string.IsNullOrWhiteSpace(codeStr))
                    break;

                await modal.DeferAsync(true);
                if (!Guid.TryParse(codeStr, out var code))
                {
                    await modal.FollowupAsync($"{codeStr} isn't a valid code! Get one in-game from the lobby at the top left of the screen.", ephemeral: true);
                }

                var author = modal.User;
                var authorId = author.Id;
                var discord = await db.RMCDiscordAccounts
                    .Include(d => d.LinkedAccount)
                    .ThenInclude(l => l.Player)
                    .ThenInclude(p => p.Patron)
                    .FirstOrDefaultAsync(a => a.Id == authorId);
                var codes = await db.RMCLinkingCodes
                    .Include(l => l.Player)
                    .ThenInclude(player => player.Patron)
                    .FirstOrDefaultAsync(p => p.Code == code);

                if (codes == null)
                {
                    await modal.FollowupAsync($"No player found with code {codeStr}, join the game server and get another code before trying again, or ask for help in another channel.", ephemeral: true);
                    break;
                }

                if (codes.CreationTime < DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)))
                {
                    await modal.FollowupAsync($"Code {codeStr} were generated too long ago, join the game server and get another code before trying again.", ephemeral: true);
                }

                if (discord?.LinkedAccount is { } linked)
                {
                    if (linked.Player.Patron is { } patron)
                        db.RMCPatrons.Remove(patron);

                    linked.Player.Patron = null;
                    db.RMCLinkedAccounts.Remove(linked);
                }

                discord ??= db.RMCDiscordAccounts.Add(new RMCDiscordAccount { Id = authorId }).Entity;
                discord.LinkedAccount = db.RMCLinkedAccounts.Add(new RMCLinkedAccount { Discord = discord }).Entity;
                discord.LinkedAccount.Player = codes.Player;

                var roles = client.GetGuild(guildId).GetUser(authorId).Roles.Select(r => r.Id).ToArray();
                var tiers = await db.RMCPatronTiers
                    .Where(t => roles.Contains(t.DiscordRole))
                    .ToListAsync();
                if (tiers.Count == 0)
                {
                    discord.LinkedAccount.Player.Patron = null;
                }
                else
                {
                    tiers.Sort((a, b) => a.Priority.CompareTo(b.Priority));
                    var tier = tiers[0];
                    discord.LinkedAccount.Player.Patron = db.RMCPatrons.Add(new RMCPatron { Tier = tier }).Entity;
                    discord.LinkedAccount.Player.Patron.Tier = tier;
                }

                db.RMCLinkedAccountLogs.Add(new RMCLinkedAccountLogs
                {
                    Discord = discord,
                    Player = discord.LinkedAccount.Player,
                });

                db.ChangeTracker.DetectChanges();
                await db.SaveChangesAsync();

                var msg = $"Linked SS14 account with name {codes.Player.LastSeenUserName}";
                if (codes.Player.Patron != null)
                    msg += $" and tier {codes.Player.Patron.Tier.Name}";

                await modal.FollowupAsync(msg, ephemeral: true);
                break;
        }
    }

    private async Task RefreshPatrons()
    {
        while (Interlocked.CompareExchange(ref Running, 1, 1) == 1)
        {
            try
            {
                var patrons = await db.RMCLinkedAccounts
                    .Include(l => l.Player)
                    .ThenInclude(p => p.Patron)
                    .ThenInclude(p => p!.Tier)
                    .ToListAsync();

                foreach (var linked in patrons)
                {
                    var user = await client.Rest.GetGuildUserAsync(Guild, linked.DiscordId);
                    if (user == null)
                    {
                        if (linked.Player.Patron != null)
                        {
                            linked.Player.Patron = null;
                            await Logger.Info($"Removed patron :{linked.DiscordId}:{linked.Player.LastSeenUserName}");
                        }

                        continue;
                    }

                    var isPatron = false;
                    foreach (var tier in _tierPriority)
                    {
                        if (user.RoleIds.Contains(tier.DiscordRole))
                        {
                            isPatron = true;
                            if (linked.Player.Patron?.Tier.DiscordRole == tier.DiscordRole)
                                break;

                            linked.Player.Patron ??= db.RMCPatrons.Add(new RMCPatron { PlayerId = linked.PlayerId }).Entity;
                            linked.Player.Patron.TierId = tier.Id;
                            await Logger.Info($"Updated patron {user.Username}:{linked.DiscordId}:{linked.Player.LastSeenUserName} with tier {tier.Name}");
                            break;
                        }
                    }

                    if (!isPatron && linked.Player.Patron != null)
                    {
                        linked.Player.Patron = null;
                        await Logger.Info($"Removed patron {user.Username}:{linked.DiscordId}:{linked.Player.LastSeenUserName}");
                    }
                }

                await db.SaveChangesAsync();
                await Task.Delay(60000);
            }
            catch (Exception e)
            {
                await Logger.Error("Error refreshing patrons", e);
            }
        }
    }
}