// SPDX-FileCopyrightText: 2025 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Discord;

namespace Content.DiscordBot;

public static class Logger
{
    public static Task Log(LogMessage msg)
    {
        return Console.Out.WriteLineAsync($"[{msg.Severity.ToString().ToUpper()}] {msg.Message}");
    }

    public static Task Info(string msg)
    {
        return Console.Out.WriteLineAsync($"[INFO] {msg}");
    }

    public static Task Error(string msg, Exception e)
    {
        return Console.Out.WriteLineAsync($"[ERROR] {msg}\n{e}");
    }
}