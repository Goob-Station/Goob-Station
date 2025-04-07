// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Manel Navola <6786088+ManelNavola@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 NuclearWinter <nukeuler123@gmail.com>
// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Schrödinger <132720404+Schrodinger71@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2024 Winkarst <74284083+Winkarst-cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 ancientpower <evafleck@gmail.com>
// SPDX-FileCopyrightText: 2020 chairbender <kwhipke1@gmail.com>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 no <165581243+pissdemon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 zumorica <zddm@outlook.es>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Server.Popups;
using Content.Shared.Administration;
using Content.Shared.GameTicking;
using Content.Shared.Mind;
using Robust.Shared.Console;
using Content.Server.GameTicking;

namespace Content.Server.Ghost
{
    [AnyCommand]
    public sealed class GhostCommand : IConsoleCommand
    {
        [Dependency] private readonly IEntityManager _entities = default!;

        public string Command => "ghost";
        public string Description => Loc.GetString("ghost-command-description");
        public string Help => Loc.GetString("ghost-command-help-text");

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var player = shell.Player;
            if (player == null)
            {
                shell.WriteLine(Loc.GetString("ghost-command-no-session"));
                return;
            }

            var gameTicker = _entities.System<GameTicker>();
            if (!gameTicker.PlayerGameStatuses.TryGetValue(player.UserId, out var playerStatus) ||
                playerStatus is not PlayerGameStatus.JoinedGame)
            {
                shell.WriteLine(Loc.GetString("ghost-command-error-lobby"));
                return;
            }

            if (player.AttachedEntity is { Valid: true } frozen &&
                _entities.HasComponent<AdminFrozenComponent>(frozen))
            {
                var deniedMessage = Loc.GetString("ghost-command-denied");
                shell.WriteLine(deniedMessage);
                _entities.System<PopupSystem>()
                    .PopupEntity(deniedMessage, frozen, frozen);
                return;
            }

            var minds = _entities.System<SharedMindSystem>();
            if (!minds.TryGetMind(player, out var mindId, out var mind))
            {
                mindId = minds.CreateMind(player.UserId);
                mind = _entities.GetComponent<MindComponent>(mindId);
            }

            if (!_entities.System<GhostSystem>().OnGhostAttempt(mindId, true, true, mind))
            {
                shell.WriteLine(Loc.GetString("ghost-command-denied"));
            }
        }
    }
}