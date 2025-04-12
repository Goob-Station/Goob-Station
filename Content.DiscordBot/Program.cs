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

using System.Text.Json;
using Content.DiscordBot;
using Content.Server.Database;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

var client = new DiscordSocketClient(new DiscordSocketConfig { GatewayIntents = GatewayIntents.All });
client.Log += Logger.Log;

string? token = null;
string? connectionString = null;
string? guildId = null;
if (File.Exists("config.json"))
{
    var config = await JsonSerializer.DeserializeAsync<Config>(File.OpenRead("config.json")) ?? new Config();
    token = config.Token;
    connectionString = config.DatabaseString;
    guildId = config.GuildID;
}

#if DEBUG
if (Environment.GetEnvironmentVariable("DISCORD_TOKEN") is { } envToken)
    token = envToken;

if (Environment.GetEnvironmentVariable("DATABASE_STRING") is { } dbString)
    connectionString = dbString;

if (Environment.GetEnvironmentVariable("GUILD_ID") is { } guildID)
    guildId = guildID;
#endif

if (string.IsNullOrWhiteSpace(token))
    throw new ArgumentException("No token found.");

if (string.IsNullOrWhiteSpace(connectionString))
    throw new ArgumentException("No database connection string found.");

if (string.IsNullOrWhiteSpace(guildId))
    throw new ArgumentException("No guild id string found.");

await client.LoginAsync(TokenType.Bot, token);
await client.StartAsync();

var builder = new DbContextOptionsBuilder<PostgresServerDbContext>();
builder.UseNpgsql(connectionString);
var db = new PostgresServerDbContext(builder.Options);
// await db.Database.MigrateAsync();

var interaction = new InteractionService(client);
var handler = new CommandHandler(client, new CommandService(), interaction, db, Convert.ToUInt64(guildId));

AppDomain.CurrentDomain.ProcessExit += (_, _) => Interlocked.Decrement(ref handler.Running);

await handler.InstallCommandsAsync();

// Block this task until the program is closed.
await Task.Delay(-1);