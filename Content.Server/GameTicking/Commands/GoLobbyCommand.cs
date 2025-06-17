// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration;
using Content.Server.GameTicking.Presets;
using Content.Shared.Administration;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Console;

namespace Content.Server.GameTicking.Commands
{
    [AdminCommand(AdminFlags.Round)]
    public sealed class GoLobbyCommand : LocalizedEntityCommands
    {
        [Dependency] private readonly IConfigurationManager _configManager = default!;
        [Dependency] private readonly GameTicker _gameTicker = default!;

        public override string Command => "golobby";

        public override void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            GamePresetPrototype? preset = null;
            var presetName = string.Join(" ", args);

            if (args.Length > 0)
            {
                if (!_gameTicker.TryFindGamePreset(presetName, out preset))
                {
                    shell.WriteLine(Loc.GetString($"cmd-forcepreset-no-preset-found", ("preset", presetName)));
                    return;
                }
            }

            _configManager.SetCVar(CCVars.GameLobbyEnabled, true);

            _gameTicker.RestartRound();

            if (preset != null)
                _gameTicker.SetGamePreset(preset);

            shell.WriteLine(Loc.GetString(preset == null ? "cmd-golobby-success" : "cmd-golobby-success-with-preset", ("preset", presetName)));
        }
    }
}