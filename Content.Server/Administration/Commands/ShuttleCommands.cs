// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.RoundEnd;
using Content.Shared.Administration;
using Content.Shared.Localizations;
using Robust.Shared.Console;

namespace Content.Server.Administration.Commands
{
    [AdminCommand(AdminFlags.Round)]
    public sealed class CallShuttleCommand : IConsoleCommand
    {
        [Dependency] private readonly IEntityManager _e = default!;

        public string Command => "callshuttle";
        public string Description => Loc.GetString("call-shuttle-command-description");
        public string Help => Loc.GetString("call-shuttle-command-help-text", ("command",Command));

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var loc = IoCManager.Resolve<ILocalizationManager>();

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (args.Length == 1 && TimeSpan.TryParseExact(args[0], ContentLocalizationManager.TimeSpanMinutesFormats, loc.DefaultCulture, out var timeSpan))
            {
                _e.System<RoundEndSystem>().RequestRoundEnd(timeSpan, shell.Player?.AttachedEntity, false);
            }
            else if (args.Length == 1)
            {
                shell.WriteLine(Loc.GetString("shell-timespan-minutes-must-be-correct"));
            }
            else
            {
                _e.System<RoundEndSystem>().RequestRoundEnd(shell.Player?.AttachedEntity, false);
            }
        }
    }

    [AdminCommand(AdminFlags.Round)]
    public sealed class RecallShuttleCommand : IConsoleCommand
    {
        [Dependency] private readonly IEntityManager _e = default!;

        public string Command => "recallshuttle";
        public string Description => Loc.GetString("recall-shuttle-command-description");
        public string Help => Loc.GetString("recall-shuttle-command-help-text", ("command",Command));

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            _e.System<RoundEndSystem>().CancelRoundEndCountdown(shell.Player?.AttachedEntity, false);
        }
    }
}