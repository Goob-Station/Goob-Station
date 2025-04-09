// SPDX-FileCopyrightText: 2023 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 LordCarve <27449516+LordCarve@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration;
using Content.Server.Administration.Logs;
using Content.Shared.Administration;
using Content.Shared.Database;
using Content.Shared.CCVar;
using Content.Server.Chat.Managers;
using Robust.Shared.Configuration;
using Robust.Shared.Console;

namespace Content.Server.Motd;

/// <summary>
/// A console command usable by any user which prints or sets the Message of the Day.
/// </summary>
[AdminCommand(AdminFlags.Moderator)]
public sealed class SetMotdCommand : LocalizedCommands
{
    [Dependency] private readonly IAdminLogManager _adminLogManager = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;

    public override string Command => "set-motd";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        string motd = "";
        var player = shell.Player;
        if (args.Length > 0)
        {
            motd = string.Join(" ", args).Trim();
            if (player != null && _chatManager.MessageCharacterLimit(player, motd))
                return; // check function prints its own error response
        }

        _configurationManager.SetCVar(CCVars.MOTD, motd); // A hook in MOTDSystem broadcasts changes to the MOTD to everyone so we don't need to do it here.
        if (string.IsNullOrEmpty(motd))
        {
            shell.WriteLine(Loc.GetString("cmd-set-motd-cleared-motd-message"));
            _adminLogManager.Add(LogType.Chat, LogImpact.Low, $"{(player == null ? "LOCALHOST" : player.Channel.UserName):Player} cleared the MOTD for the server.");
        }
        else
        {
            shell.WriteLine(Loc.GetString("cmd-set-motd-set-motd-message", ("motd", motd)));
            _adminLogManager.Add(LogType.Chat, LogImpact.Low, $"{(player == null ? "LOCALHOST" : player.Channel.UserName):Player} set the MOTD for the server to \"{motd:motd}\"");
        }
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
            return CompletionResult.FromHint(Loc.GetString("cmd-set-motd-hint-head"));
        return CompletionResult.FromHint(Loc.GetString("cmd-set-motd-hint-cont"));
    }
}