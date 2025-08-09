// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <drsmugleaf@gmail.com>
// SPDX-FileCopyrightText: 2023 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Schrödinger <132720404+Schrodinger71@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Administration;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Players;
using Robust.Server.Player;
using Robust.Shared.Console;

namespace Content.Server.Administration.Commands
{
    [AdminCommand(AdminFlags.Admin)]
    public sealed class SetMindCommand : LocalizedEntityCommands
    {
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] private readonly SharedMindSystem _mindSystem = default!;

        public override string Command => "setmind";

        public override string Description => Loc.GetString("cmd-setmind-desc", ("requiredComponent", nameof(MindContainerComponent)));

        public override void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length < 2)
            {
                shell.WriteLine(Loc.GetString("shell-wrong-arguments-number"));
                return;
            }

            if (!int.TryParse(args[0], out var entInt))
            {
                shell.WriteLine(Loc.GetString("shell-entity-uid-must-be-number"));
                return;
            }

            var ghostOverride = true;
            if (args.Length > 2)
            {
                ghostOverride = bool.Parse(args[2]);
            }

            var nent = new NetEntity(entInt);

            if (!EntityManager.TryGetEntity(nent, out var eUid))
            {
                shell.WriteLine(Loc.GetString("shell-invalid-entity-id"));
                return;
            }

            if (!EntityManager.HasComponent<MindContainerComponent>(eUid))
            {
                shell.WriteLine(Loc.GetString("cmd-setmind-target-has-no-mind-message"));
                return;
            }

            if (!_playerManager.TryGetSessionByUsername(args[1], out var session))
            {
                shell.WriteLine(Loc.GetString("shell-target-player-does-not-exist"));
                return;
            }

            // hm, does player have a mind? if not we may need to give them one
            var playerCData = session.ContentData();
            if (playerCData == null)
            {
                shell.WriteLine(Loc.GetString("cmd-setmind-target-has-no-content-data-message"));
                return;
            }

            var metadata = EntityManager.GetComponent<MetaDataComponent>(eUid.Value);

            var mind = playerCData.Mind ?? _mindSystem.CreateMind(session.UserId, metadata.EntityName);

            _mindSystem.TransferTo(mind, eUid, ghostOverride);
        }

        public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            if (args.Length == 2)
                return CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), Help);

            return CompletionResult.Empty;
        }
    }
}